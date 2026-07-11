using static go.builtin;

static partial class main_package
{
    static string TypeName(object i)
    {
        return i is null ? "<nil>" : GetGoTypeName(i.GetType());
    }
}
