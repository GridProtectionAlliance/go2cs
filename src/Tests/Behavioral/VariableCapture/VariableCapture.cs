namespace go;

using fmt = fmt_package;

public static partial class main_package {

[GoType("struct")]
private partial struct data {
    public @string name;
}

private static void printName(this data d) {
    fmt.Println("Name =", d.name);
}

private static void Main() {
    var d = new data(name: "James"u8);
    var dʗ1 = d;
    var f1 = () => d.printName();
    f1();
    d.name = "Gretchen"u8;
    f1();
}

} // end main_package
