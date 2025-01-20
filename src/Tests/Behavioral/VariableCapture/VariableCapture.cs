namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct data {
    public @string name;
}

private static void printName(this data d) {
    fmt.Println("Name =", d.name);
}

private static void Main() {
    var d = new data(name: "James"u8);
    var f1 = () => d.printName();
    f1();
    d.name = "Gretchen"u8;
    f1();
}

} // end main_package
