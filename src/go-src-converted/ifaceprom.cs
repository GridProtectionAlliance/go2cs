// package main -- go2cs converted at 2022 March 06 23:33:45 UTC
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\testdata\ifaceprom.go


namespace go;

public static partial class main_package {

    // Test of promotion of methods of an interface embedded within a
    // struct.  In particular, this test exercises that the correct
    // method is called.
public partial interface I {
    @string one();
    @string two();
}

public partial struct S : I {
    public I I;
}

private partial struct impl {
}

private static nint one(this impl _p0) {
    return 1;
}

private static @string two(this impl _p0) {
    return "two";
}

private static void Main() => func((_, panic, _) => {
    S s = default;
    s.I = new impl();
    {
        var one__prev1 = one;

        var one = s.I.one();

        if (one != 1) {
            panic(one);
        }
        one = one__prev1;

    }

    {
        var one__prev1 = one;

        one = s.one();

        if (one != 1) {
            panic(one);
        }
        one = one__prev1;

    }

    var closOne = s.I.one;
    {
        var one__prev1 = one;

        one = closOne();

        if (one != 1) {
            panic(one);
        }
        one = one__prev1;

    }

    closOne = s.one;
    {
        var one__prev1 = one;

        one = closOne();

        if (one != 1) {
            panic(one);
        }
        one = one__prev1;

    }


    {
        var two__prev1 = two;

        var two = s.I.two();

        if (two != "two") {
            panic(two);
        }
        two = two__prev1;

    }

    {
        var two__prev1 = two;

        two = s.two();

        if (two != "two") {
            panic(two);
        }
        two = two__prev1;

    }

    var closTwo = s.I.two;
    {
        var two__prev1 = two;

        two = closTwo();

        if (two != "two") {
            panic(two);
        }
        two = two__prev1;

    }

    closTwo = s.two;
    {
        var two__prev1 = two;

        two = closTwo();

        if (two != "two") {
            panic(two);
        }
        two = two__prev1;

    }

});

} // end main_package
