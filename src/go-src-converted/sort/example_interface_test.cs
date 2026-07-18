// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using sort = go.sort_package;
using go;

partial class sort_test_package {

[GoType] partial struct Person {
    public @string Name;
    public nint Age;
}

public static @string String(this Person p) {
    return fmt.Sprintf("%s: %d"u8, p.Name, p.Age);
}

[GoType("[]Person")] partial struct ByAge;

public static nint Len(this ByAge a) {
    return len(a);
}

public static void Swap(this ByAge a, nint i, nint j) {
    (a[i], a[j]) = (a[j], a[i]);
}

public static bool Less(this ByAge a, nint i, nint j) {
    return a[i].Age < a[j].Age;
}

public static void Example() {
    var people = new Person[]{
        new("Bob"u8, 31),
        new("John"u8, 42),
        new("Michael"u8, 17),
        new("Jenny"u8, 26)
    }.slice();
    fmt.Println(people);
    // There are two ways to sort a slice. First, one can define
    // a set of methods for the slice type, as with ByAge, and
    // call sort.Sort. In this first example we use that technique.
    sort.Sort(((ByAge)people));
    fmt.Println(people);
    // The other way is to use sort.Slice with a custom Less
    // function, which can be provided as a closure. In this
    // case no methods are needed. (And if they exist, they
    // are ignored.) Here we re-sort in reverse order: compare
    // the closure with ByAge.Less.
    var peopleʗ1 = people;
    sort.Slice(people, (nint i, nint j) => peopleʗ1[i].Age > peopleʗ1[j].Age);
    fmt.Println(people);
}

// Output:
// [Bob: 31 John: 42 Michael: 17 Jenny: 26]
// [Michael: 17 Jenny: 26 Bob: 31 John: 42]
// [John: 42 Bob: 31 Jenny: 26 Michael: 17]

} // end sort_test_package
