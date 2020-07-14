/*
package main

import "fmt"

func main() {
    i, j := 42, 2701

    p := &i         // point to i
    fmt.Println(*p) // read i through the pointer
    *p = 21         // set i through the pointer
    fmt.Println(i)  // see the new value of i

    p = &j         // point to j
    *p = *p / 37   // divide j through the pointer
    fmt.Println(j) // see the new value of j
}
*/
#region source
using fmt = go.fmt_package;

static class main_package
{
    static void Main() {
        var i = 42; var j = 2701;

        ref var p = ref i;      // point to i
        fmt.Println(p);         // read i through the pointer
        p = 21;                 // set i through the pointer
        fmt.Println(i);         // see the new value of i

        p = ref j;              // point to j
        p = p / 37;             // divide j through the pointer
        fmt.Println(j);         // see the new value of j
    }
}
#endregion
// This simple unsafe version works fine when variables are constrained to stack,
// since code matches original so closely, perhaps make this a conversion option:
//var i = 42; var j = 2701;

//var p = &i;            // point to i
//fmt.Println(*p);        // read i through the pointer
//*p = 21;                // set i through the pointer
//fmt.Println(i);        // see the new value of i

//p = &j;               // point to j
//*p = *p / 37;         // divide j through the pointer
//fmt.Println(j);        // see the new value of j

// If structure needs to leave the stack, start with heap-allocated instance of
// value. Code analysis should check that pointer does not escape the stack in
// which case simple local pointers can be used instead as this is optimal.
// If this is complex, then until conversion tool becomes sophisticated enough
// for this type of dynamic analysis, the following pattern will always work:
//ref var i = ref heap(43, out var i__ptr).Value;
//ref var j = ref heap(2701, out var j__ptr).Value;

//var p = i__ptr;         // point to i
//fmt.Println(p.Value);   // read i through the pointer
//p.Value = 21;           // set i through the pointer
//fmt.Println(i);         // see the new value of i

//p = j__ptr;             // point to j
//p.Value = p.Value / 37; // divide j through the pointer
//fmt.Println(j);         // see the new value of j
