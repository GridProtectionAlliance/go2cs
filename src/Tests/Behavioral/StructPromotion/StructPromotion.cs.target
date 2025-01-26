namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct Person {
    public @string name;
    public int32 age;
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
    public Person Person;
    public Employee Employee;
}

private static void Main() {
    var person = new Person(name: "Michał"u8, age: 29);
    fmt.Println(person);
    var record = new Record();
    record.name = "Michał"u8;
    record.age = 29;
    record.position = "software engineer"u8;
    fmt.Println(record);
    fmt.Println(record.name);
    fmt.Println(record.age);
    fmt.Println(record.position);
    fmt.Println(record.IsAdult());
    fmt.Println(record.IsManager());
}

} // end main_package
