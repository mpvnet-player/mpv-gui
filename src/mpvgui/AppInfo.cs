
using System.Reflection;

namespace mpvgui;

public static class AppInfo
{
    static Assembly? _asm;
    public static Assembly Asm { get => _asm ??= Assembly.GetExecutingAssembly(); }

    static AssemblyName? _assemblyName;
    public static AssemblyName AssemblyName { get => _assemblyName ??= Asm.GetName(); }

    static string? _productName;
    public static string ProductName { get => _productName ??= GetAssemblyAttribute<AssemblyProductAttribute>().Product; }

    static Version? _version;
    public static Version Version { get => _version ??= AssemblyName.Version!; }

    static string? _processPath;
    public static string ProcessPath { get => _processPath ??= Environment.ProcessPath!; }

    public static T GetAssemblyAttribute<T>() => (T)(object)Asm.GetCustomAttributes(typeof(T)).First();
}
