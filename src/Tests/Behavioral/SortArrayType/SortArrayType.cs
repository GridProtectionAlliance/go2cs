// Pre-package Comments

// Package Comments
namespace go;

// Pre-comment
using fmt = fmt_package;  // EOL comment
using sort = sort_package; // EOL comment

public static partial class main_package {

/* Post comment */
// Post comment 2
// Post comment 3

// A1 is a constant
public const nint A1 = iota;                // 0, 0
public const nint B = 0;

public const nint _ = 1;                    // 1, 100
public const nint D = 100;

// E211 is a constant
public const nint E211 = 2;                 // 2, 200
public const nint F = 200;

// Giant constant
public static readonly GoUntyped Giant = /* 1 << 100 */         // Wow
    GoUntyped.Parse("1267650600228229401496703205376");

public static readonly GoUntyped Giant2 = /* 1 << 200 */        // Wow2
    GoUntyped.Parse("1606938044258990275541962092341162602522202993782792835301376");

// String constant
public static readonly @string String = "Hello";          // Hello

public static readonly @string String2 = "World";         // World

// Float constant
public const float64 Float = 3.14;          // 3.14

private const float64 float2 = 3.14e+100;   // 3.14e100

// Giant float constant
private static readonly GoUntyped giantFloat = /* 1e309 */      // 1e309
    GoUntyped.Parse("1000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");

// A2 is a variable
public static nint A2 = 1;                  // 1, "42"
public static @string B2 = "42";

public static nint C21 = dynamicFn1();      // 3

// D21 is a variable
public static bool D21;                     // false, false
public static bool E2;

private static nint dynamicFn1() {

}

// Person is a type representing a person
// with a name, age, and shoe size
[GoType("struct")]
public partial struct Person {
    public @string Name;     // Name of the person
    [GoTag(@"Tag")]
    public nint Age;         // Age of the person
    public float32 ShoeSize; // Shoe size of the person
}

[GoType("[]Person")]
public partial struct PeopleByShoeSize {} // Person slice for shoe size sorting

[GoType("[]Person")]
public partial struct PeopleByAge {}

// Another one
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
