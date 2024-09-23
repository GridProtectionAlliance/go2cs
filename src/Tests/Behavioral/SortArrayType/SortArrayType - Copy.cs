namespace go;

using fmt = fmt_package;
using sort = sort_package;

public static partial class main_package {

[GoType("struct")]
public partial struct Person {
    public @string Name;
    public nint Age;
    public float32 ShoeSize; 
}

[GoType("[]Person")]
public partial struct PeopleByShoeSize {}

[GoType("[]Person")]
public partial struct PeopleByAge {}

public static nint Len(this PeopleByShoeSize p) {
    return len(p);
}

public static void Swap(this PeopleByShoeSize p, nint i, nint j) {
    (p[i], p[j]) = (p[j], p[i]);
}

public static bool Less(this PeopleByShoeSize p, nint i, nint j) {
    return (p[i].ShoeSize < p[j].ShoeSize);
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

private static void Main() {
    var people = new Person[] {
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
            ShoeSize: 8.50F
        )
    }.slice();

    fmt.Println(people);

    sort.Sort(new PeopleByShoeSize(people));
    fmt.Println(people);

    sort.Sort(new PeopleByAge(people));
    fmt.Println(people);
}

} // end main_package
