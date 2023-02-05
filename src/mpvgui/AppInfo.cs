
#nullable enable

using System.Reflection;

namespace mpvgui;

public class AppInfo
{
    public static Assembly Asm { get; } = Assembly.GetExecutingAssembly();
    public static AssemblyName AssemblyName { get; } = Asm.GetName();
    public static string ProductName { get; } = GetAssemblyAttribute<AssemblyProductAttribute>().Product;
    public static Version Version { get; } = AssemblyName.Version!;
    public static string ProcessPath { get; } = Environment.ProcessPath!;

    public static T GetAssemblyAttribute<T>() => (T)(object)Asm.GetCustomAttributes(typeof(T)).First();
}
