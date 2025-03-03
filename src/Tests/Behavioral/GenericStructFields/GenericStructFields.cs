namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct Result<T>
    where T : new()
{
    public T Value;
    public error Error;
}

[GoType] partial struct Container {
    public Result<nint> IntResult;
    public Result<@string> StringResult;
    public slice<Result<float64>> FloatValues;
    public map<@string, Result<bool>> Mappings;
}

internal static void Main() {
    var container = new Container(
        IntResult: new Result<nint>(
            Value: 42,
            Error: default!
        ),
        StringResult: new Result<@string>(
            Value: "success"u8,
            Error: default!
        ),
        FloatValues: new Result<float64>[]{
            new(Value: 3.14F, Error: default!),
            new(Value: 2.71F, Error: default!)
        }.slice(),
        Mappings: new map<@string, Result<bool>>{
            ["completed"u8] = new(Value: true, Error: default!),
            ["verified"u8] = new(Value: false, Error: fmt.Errorf("verification pending"u8))
        }
    );
    fmt.Printf("Int result: %d\n"u8, container.IntResult.Value);
    fmt.Printf("String result: %s\n"u8, container.StringResult.Value);
    fmt.Printf("First float value: %f\n"u8, container.FloatValues[0].Value);
    fmt.Printf("Completion status: %t\n"u8, container.Mappings["completed"u8].Value);
}

} // end main_package
