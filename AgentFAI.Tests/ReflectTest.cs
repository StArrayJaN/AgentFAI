using System.Reflection;
using AgentFAI;

namespace AgentFAI.Tests;

public class ReflectTest
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        var methods = GetMethodsWithAttributes<System.ComponentModel.DescriptionAttribute>(GetType());
        foreach (var method in methods)
        {
            Console.WriteLine($"添加工具:{method.Name}");
        }
        Assert.That(methods.Length == 4);
    }

    [System.ComponentModel.Description("a")]
    public void Method(){}
    
    [System.ComponentModel.Description("b")]
    private void Method2(){}
    
    [System.ComponentModel.Description]
    public static void Method3(){}
    
    [System.ComponentModel.Description("c")]
    private static void Method4(){}
    public static MethodInfo[] GetMethodsWithAttributes<T>(Type type) where T : Attribute
    {
        // 假设 o 是某个 Type 或 MemberInfo
        return type
            .GetMethods(BindingFlags.Public | BindingFlags.Instance |BindingFlags.Static | BindingFlags.NonPublic)
            .Where(m => m.GetCustomAttributes<T>().Any())
            .ToArray();
    }
}