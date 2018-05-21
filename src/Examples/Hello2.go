// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package utf8 implements functions and constants to support text encoded in
// UTF-8. It includes functions to translate between runes and UTF-8 byte sequences.
package main // Package comments

// More comments...
import (
	"fmt" /* comment after import */
	"math/rand" // comment after import 2
	"another"
	"test/and/two/noy"
)
// The conditions RuneError==unicode.ReplacementChar and
// MaxRune==unicode.MaxRune are verified in the tests.
// Defining them locally avoids this package depending on package unicode.

// Numbers fundamental to the encoding.
const (
	RuneError = '\uFFFD'     // the "error" Rune or "Unicode replacement character"
	RuneSelf  = 0x80         // characters below Runeself are represented as themselves in a single byte.
	MaxRune   = '\U0010FFFF' // Maximum valid Unicode code point.
	UTFMax    = 4            // maximum number of bytes of a UTF-8 encoded Unicode character.
)

const (
	RuneError2, RuneSelf2, MaxRune2 = '\uFFFD', 0x80, '\U0010FFFF'
	UTFMax2    = 4            // maximum number of bytes of a UTF-8 encoded Unicode character.
)

const (
	test = iota // hey, hey
	test2       // now, now
)

const (
	testi =  1 + iota * 1i
	testi2
)

type Person map[string]string

type Job map[string]string

type span struct {
    start int   "start field"
    end   int   "end field"
    another int "another field"
}

type User struct {
	Id   int
	Name string
}

type Employee struct {
	User       //annonymous field
	Title      string
	Department string
}


/* comment before function */
func main() {
	fmt.Println("Hello, 世界") /* eol comment */
	test(12)
    
    // Before call comment
	go DoIt("Yup!") // After call comment
	fmt.Println("My favorite number is", /* intra-param comment */ rand.Intn(10))
	fmt.Println("My second favorite number is", rand.Intn(10))
}
	/* comment after function 
		Hello!
	 */
// Test function
func test(a int, b int16, c []byte) {
	fmt.Println(a)
}

func noComment(ya string, andAnother int, string) (string message, error err) {
}

/* comment - another */
func DoIt(b string, ...int) int { // here
	fmt.Println(b)
    return 0
}

// FieldsFunc splits the string s at each run of Unicode code points c satisfying f(c)
// and returns an array of slices of s. If all code points in s satisfy f(c) or the
// string is empty, an empty slice is returned.
// FieldsFunc makes no guarantees about the order in which it calls f(c).
// If f does not return consistent results for a given c, FieldsFunc may crash.
func FieldsFunc(s string, f func(rune) bool) []string {
	// A span is used to record a slice of s of the form s[start:end].
	// The start index is inclusive and the end index is exclusive.
	type span struct {
		start int
		end   int
	}
	spans := make([]span, 0, 32)

	// Find the field start and end indices.
	wasField := false
	fromIndex := 0
	/*
	for i, rune := range s {
		if f(rune) {
			if wasField {
				spans = append(spans, span{start: fromIndex, end: i})
				wasField = false
			}
		} else {
			if !wasField {
				fromIndex = i
				wasField = true
			}
		}
	}
	*/

	// Last field might end at EOF.
	if wasField {
		spans = append(spans, span{fromIndex, len(s)})
	}

	// Create strings from recorded field indices.
	a := make([]string, len(spans))
	
	/*
	for i, span := range spans {
		a[i] = s[span.start:span.end]
	}
	*/

	return a
}
/* last comment

more

more 

more
*/
