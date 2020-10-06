package main

import "fmt"

func main() {
   var a int
   var ptr *int
   var pptr **int
   var ppptr ***int

   a = 3000

   /* take the address of var */
   ptr = &a

   /* take the address of ptr using address of operator & */
   pptr = &ptr
   ppptr = &pptr;

   /* take the value using pptr */
   fmt.Printf("Value of a = %d\n", a )
   PrintValPtr(ptr)
   fmt.Printf("Main-function return value available at *ptr = %d\n", *EscapePrintValPtr(ptr))
   fmt.Printf("Main-function updated value available at *ptr = %d\n", *ptr )
   PrintValPtr2Ptr(pptr)
   PrintValPtr2Ptr2Ptr(ppptr)

   a = 1900

   fmt.Printf("Value of a = %d\n", a )
   PrintValPtr(ptr)
   fmt.Printf("Main-function return value available at *ptr = %d\n", *EscapePrintValPtr(ptr))
   fmt.Printf("Main-function updated value available at *ptr = %d\n", *ptr )
   PrintValPtr2Ptr(pptr)
   PrintValPtr2Ptr2Ptr(ppptr)
 }

func PrintValPtr(ptr *int) {
    fmt.Printf("Value available at *ptr = %d\n", *ptr )
    *ptr++;
}

func EscapePrintValPtr(ptr *int) *int {
    fmt.Printf("Value available at *ptr = %d\n", *ptr )
    i := 99
    ptr = &i
    fmt.Printf("Intra-function updated value available at *ptr = %d\n", *ptr )
    return ptr;
}

func PrintValPtr2Ptr(pptr **int) {
    fmt.Printf("Value available at **pptr = %d\n", **pptr)
}

func PrintValPtr2Ptr2Ptr(ppptr ***int) {
    fmt.Printf("Value available at ***pptr = %d\n", ***ppptr)
}
