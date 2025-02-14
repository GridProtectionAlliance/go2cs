namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct Message {
    public @string Text;
}

public static void Print(this Message m) {
    fmt.Println(m.Text);
}

[GoType] partial interface main_Printer {
    void Print();
}

private static void Main() {
    main_Printer p = new Message("Hello, from a function-scoped interface!");
    p.Print();
}

} // end main_package
