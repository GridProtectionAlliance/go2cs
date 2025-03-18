namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct Queue<T>
    where T : new()
{
    public slice<T> items;
}

[GoRecv] internal static void Enqueue<T>(this ref Queue<T> q, T item)
    where T : new()
{
    q.items = append(q.items, item);
}

[GoRecv] internal static (T, bool) Dequeue<T>(this ref Queue<T> q)
    where T : new()
{
    T zero = default!;
    if (len(q.items) == 0) {
        return (zero, false);
    }
    var item = q.items[0];
    q.items = q.items[1..];
    return (item, true);
}

[GoRecv] internal static nint Size<T>(this ref Queue<T> q)
    where T : new()
{
    return len(q.items);
}

internal static void Main() {
    var intQueue = new Queue<nint>(nil);
    intQueue.Enqueue(10);
    intQueue.Enqueue(20);
    fmt.Printf("Int queue size: %d\n"u8, intQueue.Size());
    var stringQueue = new Queue<@string>(
        items: new @string[]{"hello", "world"}.slice()
    );
    fmt.Printf("String queue size: %d\n"u8, stringQueue.Size());
    Queue<float64> floatQueue = new Queue<float64>(
        items: new float64[]{3.14F, 2.71F, 1.618F}.slice()
    );
    fmt.Printf("Float queue size: %d\n"u8, floatQueue.Size());
    var queues = new any[]{
        new Queue<nint>(nil),
        new Queue<@string>(nil),
        new Queue<bool>(items: new bool[]{true, false}.slice())
    }.slice();
    fmt.Printf("Number of queues: %d\n"u8, len(queues));
}

} // end main_package
