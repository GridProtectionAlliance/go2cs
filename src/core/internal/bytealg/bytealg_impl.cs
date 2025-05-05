namespace go.@internal;

partial class bytealg_package
{
    public static partial slice<byte> MakeNoZero(nint n)
    {
        return new slice<byte>(new byte[n]);
    }
}
