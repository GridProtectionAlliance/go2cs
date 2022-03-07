// package main -- go2cs converted at 2022 March 06 23:33:45 UTC
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\testdata\methprom.go


namespace go;

public static partial class main_package {

    // Tests of method promotion logic.
public partial struct A {
    public nint magic;
}

public static void x(this A a) => func((_, panic, _) => {
    if (a.magic != 1) {
        panic(a.magic);
    }
});
private static ptr<A> y(this ptr<A> _addr_a) {
    ref A a = ref _addr_a.val;

    return _addr_a!;
}

public partial struct B {
    public nint magic;
}

public static void p(this B b) => func((_, panic, _) => {
    if (b.magic != 2) {
        panic(b.magic);
    }
});
private static void q(this ptr<B> _addr_b) => func((_, panic, _) => {
    ref B b = ref _addr_b.val;

    if (b != theC.B) {
        panic("oops");
    }
});

public partial interface I {
    void f();
}

private partial struct impl {
    public nint magic;
}

private static void f(this impl i) => func((_, panic, _) => {
    if (i.magic != 3) {
        panic("oops");
    }
});

public partial struct C : I {
    public ref A A => ref A_val;
    public ref ptr<B> ptr<B> => ref ptr<B>_ptr;
    public I I;
}

private static void assert(bool cond) => func((_, panic, _) => {
    if (!cond) {
        panic("failed");
    }
});

private static C theC = new C(A:A{1},B:&B{2},I:impl{3},);

private static ptr<C> addr() {
    return _addr__addr_theC!;
}

private static C value() {
    return theC;
}

private static void Main() => func((_, panic, _) => { 
    // address
    addr().x();
    if (addr().y() != _addr_theC.A) {
        panic("oops");
    }
    addr().p();
    addr().q();
    addr().f(); 

    // addressable value
    C c = value();
    c.x();
    if (c.y() != _addr_c.A) {
        panic("oops");
    }
    c.p();
    c.q();
    c.f(); 

    // non-addressable value
    value().x(); 
    // value().y() // not in method set
    value().p();
    value().q();
    value().f();

});

} // end main_package
