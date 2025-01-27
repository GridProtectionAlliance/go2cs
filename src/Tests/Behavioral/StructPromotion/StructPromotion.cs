namespace go;

using fmt = fmt_package;
using strings = strings_package;

partial class main_package {

[GoType] partial struct Person {
    public @string name;
    public int32 age;
}

public static bool IsDr(this Person p) {
    return strings.HasPrefix(p.name, "Dr"u8);
}

public static bool IsAdult(this Person p) {
    return p.age >= 18;
}

[GoType] partial struct Employee {
    public @string position;
}

public static bool IsManager(this Employee e) {
    return e.position == "manager"u8;
}

[GoType] partial struct Record {
    public partial ref Person Person { get; }
    public partial ref Employee Employee { get; }
}

public static bool IsDr(this Record p) {
    return strings.HasPrefix(p.name, "Dr"u8) && p.age > 18;
}

private static void Main() {
    var person = new Person(name: "Dr. Michał"u8, age: 29);
    fmt.Println(person);
    fmt.Println(person.IsDr());
    var record = new Record(nil);
    record.name = "Dr. Michał"u8;
    record.age = 18;
    record.position = "software engineer"u8;
    fmt.Println(record);
    fmt.Println(record.name);
    fmt.Println(record.age);
    fmt.Println(record.position);
    fmt.Println(record.IsAdult());
    fmt.Println(record.IsManager());
    fmt.Println(record.IsDr());
}

} // end main_package
