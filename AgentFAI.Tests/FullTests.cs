using System.Reflection;
using AgentFAI;
using AgentFAI.Extensions;

namespace AgentFAI.Tests;

public class FullTests
{
    private int a = 4;
    private static double b = 6d;
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestReflect()
    {
        var methods = GetMethodsWithAttributes<System.ComponentModel.DescriptionAttribute>(GetType());
        foreach (var method in methods)
        {
            Console.WriteLine($"添加工具:{method.Name}");
        }
        Assert.That(methods.Length == 4);
    }
    
    [Test]
    public void TestReflectFieldRef()
    {
        ref int f = ref this.FieldReference<int>("a");
        f = 8;
        Assert.That(a == f);
    }
    
    [Test]
    public void TestStaticReflectFieldRef()
    {
        ref var f = ref this.FieldReference<double>("b");
        f = 8;
        Assert.That(b == f);
    }

    [Test]
    public async Task TestTask()
    {
        SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        int threadId = Thread.CurrentThread.ManagedThreadId;
        Assert.That(Thread.CurrentThread.ManagedThreadId == threadId);
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