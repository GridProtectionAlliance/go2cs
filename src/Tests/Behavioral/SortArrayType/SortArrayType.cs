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

private const float32 w = 1;     // Other

public const nint @_addr_X = 1;  // One

public const nint Y = 2;         // Two

public const nint Z = 3;         // Three

// A1 is a constant
public const nint A1 = iota;              // 0, 0
public const nint B = 0;

public const nint _ = 1;                  // 1, 100
public const nint D = 100;

// E211 is a constant
public const nint E211 = 2;               // 2, 200
public const nint F = 200;

// Giant constant
public static readonly GoUntyped Giant = /* 1 << 100 */             // Wow
    GoUntyped.Parse("1267650600228229401496703205376");

public static readonly GoUntyped Giant2 = /* 1 << 200 */            // Wow2
    GoUntyped.Parse("1606938044258990275541962092341162602522202993782792835301376");

// String constant
public static readonly @string String = "Hello"u8;    // Hello

public static readonly @string String2 = "World"u8;   // World

public static readonly @string String3 = "世界 \u0053\u004a3"u8; // Extra

// Float constant
public const float64 Float = 3.14;        // 3.14

public const float64 Float2 = 3.14e+100;  // 3.14e100

// Giant float constant
public static readonly GoUntyped GiantFloat = /* 1e309 */           // 1e309
    GoUntyped.Parse("1000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");

// MultiLine constant (spaces EOL)
public static readonly @string MultiLine = """
        Line1 /123
        Line2 ""Yo""
        Line3
        """u8;

// MultiLine2 constant (no spaces EOL)
public static readonly @string MultiLine2 = """"
        Line1 /123
        Line2 """Yo"""
        Line3
""""u8;

// MultiLine3 constant (no newline at start)
public static readonly @string MultiLine3 = """
Line1
        Line2
        "Yo"
        Line3
"""u8;

// A2 is a variable
public static nint A2 = 1;                // 1, "42"
public static @string B2 = "42"u8;

public static nint C21 = dynamicFn1();    // 3

// D21 is a variable
public static bool D21;                   // false, false
public static bool E2;

public static bool @Ta_package = false;   // special ID

private static bool otherID = true;       // other ID

private static nint dynamicFn1() {
    return 4;
}
/* interface {
	Pos() int	// Pos is a method
	End12() int	// End12 is a method
} */

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
    return len(p);
}

public static void Swap(this PeopleByShoeSize p, nint i, nint j) {
    /* p[i], p[j] = p[j], p[i] */

}

public static bool Less(this PeopleByShoeSize p, nint i, nint j) {
    return (p[i].ShoeSize < p[j].ShoeSize);
}

public static nint Len(this PeopleByAge p) {
    return len(p);
}

public static void Swap(this PeopleByAge p, nint i, nint j) {
    /* p[i], p[j] = p[j], p[i] */

}

public static bool Less(this PeopleByAge p, nint i, nint j) {
    return (p[i].Age < p[j].Age);
}

private static void Main() {
    /* x := "Hello, 世界 \123\1123" */
    /* fmt.Println(x) */
    /* people := []Person{
	{
		Name:		"Person1",
		Age:		26,
		ShoeSize:	8,
	},
	{
		Name:		"Person2",
		Age:		21,
		ShoeSize:	4,
	},
	{
		Name:		"Person3",
		Age:		15,
		ShoeSize:	9,
	},
	{
		Name:		"Person4",
		Age:		45,
		ShoeSize:	15,
	},
	{
		Name:		"Person5",
		Age:		25,
		ShoeSize:	8.5,
	}} */
    /* fmt.Println(people) */
    /* sort.Sort(PeopleByShoeSize(people)) */
    /* fmt.Println(people) */
    /* sort.Sort(PeopleByAge(people)) */
    /* fmt.Println(people) */

}

// Post code comments


} // end main_package
