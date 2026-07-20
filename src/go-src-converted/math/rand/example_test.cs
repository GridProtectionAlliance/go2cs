// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math;

using fmt = fmt_package;
using rand = go.math.rand_package;
using os = os_package;
using strings = strings_package;
using tabwriter = text.tabwriter_package;
using go.math;
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
    fmt.Println("Magic 8-Ball says:", answers[rand.Intn(len(answers))]);
}

// This example shows the use of each of the methods on a *Rand.
// The use of the global functions is the same, without the receiver.
public static void Example_rand() => func((defer, recover) => {
    // Create and seed the generator.
    // Typically a non-fixed seed should be used, such as time.Now().UnixNano().
    // Using a fixed seed will produce the same output on every run.
    var r = rand.New(rand.NewSource(99));
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
    // Int31, Int63, and Uint32 generate values of the given width.
    // The Int method (not shown) is like either Int31 or Int63
    // depending on the size of 'int'.
    show("Int31"u8, r.Int31(), r.Int31(), r.Int31());
    show("Int63"u8, r.Int63(), r.Int63(), r.Int63());
    show("Uint32"u8, r.Uint32(), r.Uint32(), r.Uint32());
    // Intn, Int31n, and Int63n limit their output to be < n.
    // They do so more carefully than using r.Int()%n.
    show("Intn(10)"u8, r.Intn(10), r.Intn(10), r.Intn(10));
    show("Int31n(10)"u8, r.Int31n(10), r.Int31n(10), r.Int31n(10));
    show("Int63n(10)"u8, r.Int63n(10), r.Int63n(10), r.Int63n(10));
    // Perm generates a random permutation of the numbers [0, n).
    show("Perm"u8, r.Perm(5), r.Perm(5), r.Perm(5));
});

// Output:
// Float32     0.2635776           0.6358173           0.6718283
// Float64     0.628605430454327   0.4504798828572669  0.9562755949377957
// ExpFloat64  0.3362240648200941  1.4256072328483647  0.24354758816173044
// NormFloat64 0.17233959114940064 1.577014951434847   0.04259129641113857
// Int31       1501292890          1486668269          182840835
// Int63       3546343826724305832 5724354148158589552 5239846799706671610
// Uint32      2760229429          296659907           1922395059
// Intn(10)    1                   2                   5
// Int31n(10)  4                   7                   8
// Int63n(10)  7                   6                   3
// Perm        [1 4 2 3 0]         [4 2 1 3 0]         [1 2 4 0 3]
public static void ExamplePerm() {
    foreach (var (_, value) in rand.Perm(3)) {
        fmt.Println(value);
    }
}

// Unordered output: 1
// 2
// 0
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

public static void ExampleIntn() {
    fmt.Println(rand.Intn(100));
    fmt.Println(rand.Intn(100));
    fmt.Println(rand.Intn(100));
}

} // end rand_test_package
