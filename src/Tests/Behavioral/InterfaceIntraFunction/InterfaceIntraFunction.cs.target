namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct Message {
    public @string Text;
}

public static void Print(this Message m) {
    fmt.Println(m.Text);
}

private static void Main() {
    [GoType] partial interface Printer {
        void Print();
    }

    Printer p = new Message("Hello, from a function-scoped interface!");
    p.Print();
}

} // end main_package
