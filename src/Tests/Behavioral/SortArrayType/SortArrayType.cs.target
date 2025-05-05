namespace go;

using fmt = fmt_package;
using sort = sort_package;

partial class main_package {

internal const float32 w = 1;
internal static readonly UntypedInt _addr_X = 1;
public static readonly UntypedInt Y = 2;
public static readonly UntypedInt Z = 3;

public static readonly UntypedInt A1 = iota;
public static readonly UntypedInt B = /* iota * 100 */ 0;
internal static readonly UntypedInt _ = 1;
public static readonly UntypedInt D = 100;
public static readonly UntypedInt E211 = 2;
public static readonly UntypedInt F = 200;
public static readonly GoUntyped Giant = /* 1 << 100 */
    GoUntyped.Parse("1267650600228229401496703205376");
public static readonly GoUntyped Giant2 = /* 1 << 200 */
    GoUntyped.Parse("1606938044258990275541962092341162602522202993782792835301376");
public static readonly @string String = "Hello"u8;
public static readonly @string String2 = "World"u8;
public static readonly @string String3 = "世界 \u0053\u004a3"u8;
public static readonly UntypedFloat Float = 3.14;
public static readonly UntypedFloat Float2 = 3.14e+100;
public static readonly GoUntyped GiantFloat = /* 1e309 */
    GoUntyped.Parse("1000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");
public static readonly @string MultiLine = """

        Line1 /123
        Line2 ""Yo""
        Line3
        
"""u8;
public static readonly @string MultiLine2 = """"

        Line1 /123
        Line2 """Yo"""
        Line3
""""u8;
public static readonly @string MultiLine3 = """
Line1
        Line2
        "Yo"
        Line3
"""u8;

public static nint A2 = 1;
public static @string B2 = "42"u8;
public static nint C21 = dynamicFn1();
public static bool D21;
public static bool E2;
public static bool ΔTa_package = false;
internal static bool otherID = true;

internal static nint dynamicFn1() {
    return 4;
}

[GoType] partial interface NodeR {
    nint Pos();
    nint End12();
    @string Name(nint offset);
}

[GoType] partial struct Person {
    public @string Name;
    [GoTag(@"json:""Tag""")]
    public nint Age;
    public float32 ShoeSize;
}

[GoType("[]Person")] partial struct PeopleByShoeSize;

[GoType("[]Person")] partial struct PeopleByAge;

public static nint Len(this PeopleByShoeSize p) {
    return len(p);
}

public static void Swap(this PeopleByShoeSize p, nint i, nint j) {
    (p[i], p[j]) = (p[j], p[i]);
}

public static bool /*a*/ Less(this PeopleByShoeSize p, nint i, nint j) {
    bool a = default!;

    a = (p[i].ShoeSize < p[j].ShoeSize);
    return a;
}

public static nint Len(this PeopleByAge p) {
    return len(p);
}

public static void Swap(this PeopleByAge p, nint i, nint j) {
    (p[i], p[j]) = (p[j], p[i]);
}

public static bool Less(this PeopleByAge p, nint i, nint j) {
    return (p[i].Age < p[j].Age);
}

public static (nint E2, @string p) Testing() {
    nint E2 = default!;
    @string p = default!;

    fmt.Println(E2);
    E2 = 1;
    nint B2 = 2;
    fmt.Println(E2, B2);
    p = "Hello"u8;
    {
        nint E2Δ1 = 99;
        nint B2Δ1 = 199;
        fmt.Println(E2Δ1, B2Δ1);
    }
    {
        E2 = 100;
        nint B2Δ2 = 200;
        fmt.Println(E2, B2Δ2);
    }
    fmt.Println(E2, B2);
    return (E2, p);
}

internal static void Main() {
    fmt.Println(MultiLine);
    fmt.Println(MultiLine2);
    fmt.Println(MultiLine3);
    Testing();
    @string x = "Hello, 世界 \u0053\u004a3"u8;
    fmt.Println(x);
    var people = new Person[]{
        new(
            Name: "Person1"u8,
            Age: 26,
            ShoeSize: 8
        ),
        new(
            Name: "Person2"u8,
            Age: 21,
            ShoeSize: 4
        ),
        new(
            Name: "Person3"u8,
            Age: 15,
            ShoeSize: 9
        ),
        new(
            Name: "Person4"u8,
            Age: 45,
            ShoeSize: 15
        ),
        new(
            Name: "Person5"u8,
            Age: 25,
            ShoeSize: 8.5F
        )
    }.slice();
    fmt.Println(people);
    sort.Sort(((PeopleByShoeSize)people));
    fmt.Println(people);
    sort.Sort(((PeopleByAge)people));
    fmt.Println(people);
    x = """

        SELECT *
        FROM 
"""u8 + "`Role`"u8;
}

} // end main_package
