// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math.rand;

using fmt = fmt_package;
using rand = global::go.math.rand.rand_package;
using os = os_package;
using strings = strings_package;
using tabwriter = text.tabwriter_package;
using time = time_package;
using global::go.math.rand;
using io = io_package;
using text;

partial class rand_test_package {

// These tests serve as an example but also make sure we don't change
// the output of the random number generator when given a fixed seed.
public static void Example() {
    var answers = new @string[]{
        "It is certain",
        "It is decidedly so",
        "Without a doubt",
        "Yes definitely",
        "You may rely on it",
        "As I see it yes",
        "Most likely",
        "Outlook good",
        "Yes",
        "Signs point to yes",
        "Reply hazy try again",
        "Ask again later",
        "Better not tell you now",
        "Cannot predict now",
        "Concentrate and ask again",
        "Don't count on it",
        "My reply is no",
        "My sources say no",
        "Outlook not so good",
        "Very doubtful"
    }.slice();
    fmt.Println("Magic 8-Ball says:", answers[rand.IntN(len(answers))]);
}

// This example shows the use of each of the methods on a *Rand.
// The use of the global functions is the same, without the receiver.
public static void Example_rand() => func((defer, recover) => {
    // Create and seed the generator.
    // Typically a non-fixed seed should be used, such as Uint64(), Uint64().
    // Using a fixed seed will produce the same output on every run.
    var r = rand.New(new rand.PCGжSource(rand.NewPCG(1, 2)));
    // The tabwriter here helps us generate aligned output.
    var w = tabwriter.NewWriter(new os.FileжWriter(os.Stdout), 1, 1, 1, (rune)' ', 0);
    var wʗ1 = w;
    defer(() => wʗ1.Flush());
    var wʗ2 = w;
    var show = (@string name, any v1, any v2, any v3) => {
        fmt.Fprintf(new tabwriter_WriterжWriter(wʗ2), "%s\t%v\t%v\t%v\n"u8, name, v1, v2, v3);
    };
    // Float32 and Float64 values are in [0, 1).
    show("Float32"u8, r.Float32(), r.Float32(), r.Float32());
    show("Float64"u8, r.Float64(), r.Float64(), r.Float64());
    // ExpFloat64 values have an average of 1 but decay exponentially.
    show("ExpFloat64"u8, r.ExpFloat64(), r.ExpFloat64(), r.ExpFloat64());
    // NormFloat64 values have an average of 0 and a standard deviation of 1.
    show("NormFloat64"u8, r.NormFloat64(), r.NormFloat64(), r.NormFloat64());
    // Int32, Int64, and Uint32 generate values of the given width.
    // The Int method (not shown) is like either Int32 or Int64
    // depending on the size of 'int'.
    show("Int32"u8, r.Int32(), r.Int32(), r.Int32());
    show("Int64"u8, r.Int64(), r.Int64(), r.Int64());
    show("Uint32"u8, r.Uint32(), r.Uint32(), r.Uint32());
    // IntN, Int32N, and Int64N limit their output to be < n.
    // They do so more carefully than using r.Int()%n.
    show("IntN(10)"u8, r.IntN(10), r.IntN(10), r.IntN(10));
    show("Int32N(10)"u8, r.Int32N(10), r.Int32N(10), r.Int32N(10));
    show("Int64N(10)"u8, r.Int64N(10), r.Int64N(10), r.Int64N(10));
    // Perm generates a random permutation of the numbers [0, n).
    show("Perm"u8, r.Perm(5), r.Perm(5), r.Perm(5));
});

// Output:
// Float32     0.95955694          0.8076733            0.8135684
// Float64     0.4297927436037299  0.797802349388613    0.3883664855410056
// ExpFloat64  0.43463410545541104 0.5513632046504593   0.7426404617374481
// NormFloat64 -0.9303318111676635 -0.04750789419852852 0.22248301107582735
// Int32       2020777787          260808523            851126509
// Int64       5231057920893523323 4257872588489500903  158397175702351138
// Uint32      314478343           1418758728           208955345
// IntN(10)    6                   2                    0
// Int32N(10)  3                   7                    7
// Int64N(10)  8                   9                    4
// Perm        [0 3 1 4 2]         [4 1 2 0 3]          [4 3 2 0 1]
public static void ExamplePerm() {
    foreach (var (_, value) in rand.Perm(3)) {
        fmt.Println(value);
    }
}

// Unordered output: 1
// 2
// 0
public static void ExampleN() {
    // Print an int64 in the half-open interval [0, 100).
    fmt.Println(rand.N((int64)100));
    // Sleep for a random duration between 0 and 100 milliseconds.
    time.Sleep(rand.N(100 * time.Millisecond));
}

public static void ExampleShuffle() {
    var words = strings.Fields("ink runs from the corners of my mouth"u8);
    var wordsʗ1 = words;
    rand.Shuffle(len(words), (nint i, nint j) => {
        (wordsʗ1[i], wordsʗ1[j]) = (wordsʗ1[j], wordsʗ1[i]);
    });
    fmt.Println(words);
}

public static void ExampleShuffle_slicesInUnison() {
    var numbers = slice<byte>("12345"u8);
    var letters = slice<byte>("ABCDE"u8);
    // Shuffle numbers, swapping corresponding entries in letters at the same time.
    var lettersʗ1 = letters;
    var numbersʗ1 = numbers;
    rand.Shuffle(len(numbers), (nint i, nint j) => {
        (numbersʗ1[i], numbersʗ1[j]) = (numbersʗ1[j], numbersʗ1[i]);
        (lettersʗ1[i], lettersʗ1[j]) = (lettersʗ1[j], lettersʗ1[i]);
    });
    foreach (var (i, _) in numbers) {
        fmt.Printf("%c: %c\n"u8, letters[i], numbers[i]);
    }
}

public static void ExampleIntN() {
    fmt.Println(rand.IntN(100));
    fmt.Println(rand.IntN(100));
    fmt.Println(rand.IntN(100));
}

} // end rand_test_package
