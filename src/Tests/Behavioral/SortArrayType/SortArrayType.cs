// Pre-package Comments

// Package Comments
namespace go;

// Pre-comment
using fmt = fmt_package;  // EOL comment
using sort = sort_package; // EOL comment
using System.ComponentModel;

public static partial class main_package {


/* Post comment */
// Post comment 2
// Post comment 3


// A1 is a constant
public const nint A1 = 0;                  // 0, 0
public const nint B = 0;

public const nint _ = 1;                   // 1, 100
public const nint D = 100;

// E211 is a constant
public const nint E211 = 2;                // 2, 200
public const nint F = 200;

// A2 is a variable
public static nint A2 = 1;                 // 1, "42"
public static @string B2 = "42";

public static nint C21 = dynamicFn();      // 3

// D21 is a variable
public static bool D21;                    // false, false
public static bool E2;

private static nint dynamicFn() {

}
// Person is a type representing a person
// with a name, age, and shoe size

[GoType("struct")]
public partial struct Person {
    public @string Name;     // Name of the person
    [Description(@"Tag")]
    public nint Age;         // Age of the person
    public float32 ShoeSize; // Shoe size of the person
}
// Another one

[GoType("[]Person")]
public partial struct PeopleByShoeSize {} // Person slice for shoe size sorting

[GoType("[]Person")]
public partial struct PeopleByAge {}

public static nint Len(this PeopleByShoeSize p) {

}

public static void Swap(this PeopleByShoeSize p, nint i, nint j) {

}

public static bool Less(this PeopleByShoeSize p, nint i, nint j) {

}

public static nint Len(this PeopleByAge p) {

}

public static void Swap(this PeopleByAge p, nint i, nint j) {

}

public static bool Less(this PeopleByAge p, nint i, nint j) {

}

private static void Main() {

}

// Post code comments

} // end main_package
