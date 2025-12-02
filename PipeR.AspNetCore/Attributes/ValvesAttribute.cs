namespace PipeR.AspNetCore.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class ValvesAttribute(params Type[] valveTypes) : Attribute
{
    public Type[] ValveTypes { get; } = valveTypes;
}
