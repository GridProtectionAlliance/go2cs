// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using Δmath = math_package;
using sort = go.sort_package;
using go;

partial class sort_test_package {

public static void ExampleInts() {
    var s = new nint[]{5, 2, 6, 3, 1, 4}.slice();
    // unsorted
    sort.Ints(s);
    fmt.Println(s);
}

// Output: [1 2 3 4 5 6]
public static void ExampleIntsAreSorted() {
    var s = new nint[]{1, 2, 3, 4, 5, 6}.slice();
    // sorted ascending
    fmt.Println(sort.IntsAreSorted(s));
    s = new nint[]{6, 5, 4, 3, 2, 1}.slice();
    // sorted descending
    fmt.Println(sort.IntsAreSorted(s));
    s = new nint[]{3, 2, 4, 1, 5}.slice();
    // unsorted
    fmt.Println(sort.IntsAreSorted(s));
}

// Output: true
// false
// false
public static void ExampleFloat64s() {
    var s = new float64[]{5.2D, -1.3D, 0.7D, -3.8D, 2.6D}.slice();
    // unsorted
    sort.Float64s(s);
    fmt.Println(s);
    s = new float64[]{Δmath.Inf(1), Δmath.NaN(), Δmath.Inf(-1), 0.0D}.slice();
    // unsorted
    sort.Float64s(s);
    fmt.Println(s);
}

// Output: [-3.8 -1.3 0.7 2.6 5.2]
// [NaN -Inf 0 +Inf]
public static void ExampleFloat64sAreSorted() {
    var s = new float64[]{0.7D, 1.3D, 2.6D, 3.8D, 5.2D}.slice();
    // sorted ascending
    fmt.Println(sort.Float64sAreSorted(s));
    s = new float64[]{5.2D, 3.8D, 2.6D, 1.3D, 0.7D}.slice();
    // sorted descending
    fmt.Println(sort.Float64sAreSorted(s));
    s = new float64[]{5.2D, 1.3D, 0.7D, 3.8D, 2.6D}.slice();
    // unsorted
    fmt.Println(sort.Float64sAreSorted(s));
}

// Output: true
// false
// false
public static void ExampleReverse() {
    var s = new nint[]{5, 2, 6, 3, 1, 4}.slice();
    // unsorted
    sort.Sort(sort.Reverse(((sort.IntSlice)s)));
    fmt.Println(s);
}

[GoType("dyn")] partial struct ExampleSlice_people {
    public @string Name;
    public nint Age;
}

// Output: [6 5 4 3 2 1]
public static void ExampleSlice() {
    var people = new ExampleSlice_people[]{
        new("Gopher"u8, 7),
        new("Alice"u8, 55),
        new("Vera"u8, 24),
        new("Bob"u8, 75)
    }.slice();
    var peopleʗ1 = people;
    sort.Slice(people, (nint i, nint j) => peopleʗ1[i].Name < peopleʗ1[j].Name);
    fmt.Println("By name:", people);
    var peopleʗ3 = people;
    sort.Slice(people, (nint i, nint j) => peopleʗ3[i].Age < peopleʗ3[j].Age);
    fmt.Println("By age:", people);
}

[GoType("dyn")] partial struct ExampleSliceStable_people {
    public @string Name;
    public nint Age;
}

// Output: By name: [{Alice 55} {Bob 75} {Gopher 7} {Vera 24}]
// By age: [{Gopher 7} {Vera 24} {Alice 55} {Bob 75}]
public static void ExampleSliceStable() {
    var people = new ExampleSliceStable_people[]{
        new("Alice"u8, 25),
        new("Elizabeth"u8, 75),
        new("Alice"u8, 75),
        new("Bob"u8, 75),
        new("Alice"u8, 75),
        new("Bob"u8, 25),
        new("Colin"u8, 25),
        new("Elizabeth"u8, 25)
    }.slice();
    // Sort by name, preserving original order
    var peopleʗ1 = people;
    sort.SliceStable(people, (nint i, nint j) => peopleʗ1[i].Name < peopleʗ1[j].Name);
    fmt.Println("By name:", people);
    // Sort by age preserving name order
    var peopleʗ3 = people;
    sort.SliceStable(people, (nint i, nint j) => peopleʗ3[i].Age < peopleʗ3[j].Age);
    fmt.Println("By age,name:", people);
}

// Output: By name: [{Alice 25} {Alice 75} {Alice 75} {Bob 75} {Bob 25} {Colin 25} {Elizabeth 75} {Elizabeth 25}]
// By age,name: [{Alice 25} {Bob 25} {Colin 25} {Elizabeth 25} {Alice 75} {Alice 75} {Bob 75} {Elizabeth 75}]
public static void ExampleStrings() {
    var s = new @string[]{"Go", "Bravo", "Gopher", "Alpha", "Grin", "Delta"}.slice();
    sort.Strings(s);
    fmt.Println(s);
}

// Output: [Alpha Bravo Delta Go Gopher Grin]

} // end sort_test_package
