// package main -- go2cs converted at 2022 March 06 23:33:44 UTC
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\testdata\fieldprom.go


namespace go;

public static partial class main_package {

    // Tests of field promotion logic.
public partial struct A {
    public nint x;
    public ptr<nint> y;
}

public partial struct B {
    public nint p;
    public ptr<nint> q;
}

public partial struct C {
    public ref A A => ref A_val;
    public ref ptr<B> ptr<B> => ref ptr<B>_ptr;
}

public partial struct D {
    public nint a;
    public ref C C => ref C_val;
}

private static void assert(bool cond) => func((_, panic, _) => {
    if (!cond) {
        panic("failed");
    }
});

private static void f1(C c) {
    assert(c.x == c.A.x);
    assert(c.y == c.A.y);
    assert(_addr_c.x == _addr_c.A.x);
    assert(_addr_c.y == _addr_c.A.y);

    assert(c.p == c.B.p);
    assert(c.q == c.B.q);
    assert(_addr_c.p == _addr_c.B.p);
    assert(_addr_c.q == _addr_c.B.q);

    c.x = 1;
    c.y.val = 1;
    c.p = 1;
    c.q.val = 1;
}

private static void f2(ptr<C> _addr_c) {
    ref C c = ref _addr_c.val;

    assert(c.x == c.A.x);
    assert(c.y == c.A.y);
    assert(_addr_c.x == _addr_c.A.x);
    assert(_addr_c.y == _addr_c.A.y);

    assert(c.p == c.B.p);
    assert(c.q == c.B.q);
    assert(_addr_c.p == _addr_c.B.p);
    assert(_addr_c.q == _addr_c.B.q);

    c.x = 1;
    c.y.val = 1;
    c.p = 1;
    c.q.val = 1;
}

private static void f3(D d) {
    assert(d.x == d.C.A.x);
    assert(d.y == d.C.A.y);
    assert(_addr_d.x == _addr_d.C.A.x);
    assert(_addr_d.y == _addr_d.C.A.y);

    assert(d.p == d.C.B.p);
    assert(d.q == d.C.B.q);
    assert(_addr_d.p == _addr_d.C.B.p);
    assert(_addr_d.q == _addr_d.C.B.q);

    d.x = 1;
    d.y.val = 1;
    d.p = 1;
    d.q.val = 1;
}

private static void f4(ptr<D> _addr_d) {
    ref D d = ref _addr_d.val;

    assert(d.x == d.C.A.x);
    assert(d.y == d.C.A.y);
    assert(_addr_d.x == _addr_d.C.A.x);
    assert(_addr_d.y == _addr_d.C.A.y);

    assert(d.p == d.C.B.p);
    assert(d.q == d.C.B.q);
    assert(_addr_d.p == _addr_d.C.B.p);
    assert(_addr_d.q == _addr_d.C.B.q);

    d.x = 1;
    d.y.val = 1;
    d.p = 1;
    d.q.val = 1;
}

private static void Main() {
    ref nint y = ref heap(123, out ptr<nint> _addr_y);
    ref C c = ref heap(new C(A{x:42,y:&y},&B{p:42,q:&y},), out ptr<C> _addr_c);

    assert(_addr_c.x == _addr_c.A.x);

    f1(c);
    f2(_addr_c);

    ref D d = ref heap(new D(C:c), out ptr<D> _addr_d);
    f3(d);
    f4(_addr_d);
}

} // end main_package
