namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct Stack<T>
    where T : /* ~int | ~string */ IAdditionOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    public slice<T> elements;
}

[GoRecv] public static void Push<T>(this ref Stack<T> s, T element)
    where T : /* ~int | ~string */ IAdditionOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    s.elements = append(s.elements, element);
}

[GoRecv] public static (T, bool) Pop<T>(this ref Stack<T> s)
    where T : /* ~int | ~string */ IAdditionOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    T zero = default!;
    if (len(s.elements) == 0) {
        return (zero, false);
    }
    nint index = len(s.elements) - 1;
    var element = s.elements[index];
    s.elements = s.elements[..(int)(index)];
    return (element, true);
}

public static slice<U> MapElements<T, U>(ж<Stack<T>> Ꮡs, Func<T, U> mapper)
    where T : /* ~int */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
    where U : new()
{
    ref var s = ref Ꮡs.val;

    var result = new slice<U>(0, len(s.elements));
    foreach (var (_, elem) in s.elements) {
        result = append(result, mapper(elem));
    }
    return result;
}

internal static void Main() {
    var intStack = new Stack<nint>(nil);
    intStack.Push(10);
    intStack.Push(20);
    var (val, _) = intStack.Pop();
    fmt.Printf("Popped from int stack: %d\n"u8, val);
    var stringStack = new Stack<@string>(nil);
    stringStack.Push("hello"u8);
    stringStack.Push("world"u8);
    var (text, _) = stringStack.Pop();
    fmt.Printf("Popped from string stack: %s\n"u8, text);
}

} // end main_package
