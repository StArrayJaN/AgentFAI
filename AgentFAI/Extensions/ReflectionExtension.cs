using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace AgentFAI.Extensions;

public static class ReflectionExtension
{
    extension(object o)
    {
        public void SetField(string fieldName, object value)
        {
            var field = o.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            field?.SetValue(o, value);
        }
        
        public T GetField<T>(string fieldName)
        {
            var field = o.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            return (T)field?.GetValue(o);
        }
        
        public void SetProperty(string propertyName, object value)
        {
            var property = o.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            property?.SetValue(o, value);
        }

        public T GetProperty<T>(string propertyName)
        {
            var property = o.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            return (T)property?.GetValue(o);
        }
        
        public void CallVoidMethod(string methodName, params object[] args)
        {
            var method = o.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            method?.Invoke(o, args);
        }
        
        public T CallMethod<T>(string methodName, params object[] args)
        {
            var method = o.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            return (T)method?.Invoke(o, args);
        }
        
        // Static methods
        public void SetStaticField(string fieldName, object value)
        {
            var field = o.GetType().GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            field?.SetValue(null, value);
        }
        
        public T GetStaticField<T>(string fieldName)
        {
            var field = o.GetType().GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            return (T)field?.GetValue(null);
        }
        
        public void SetStaticProperty(string propertyName, object value)
        {
            var property = o.GetType().GetProperty(propertyName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            property?.SetValue(null, value);
        }
        
        public T GetStaticProperty<T>(string propertyName)
        {
            var property = o.GetType().GetProperty(propertyName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            return (T)property?.GetValue(null);
        }
        
        public void CallStaticVoidMethod(string methodName, params object[] args)
        {
            var method = o.GetType().GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            method?.Invoke(null, args);
        }

        public T CallStaticMethod<T>(string methodName, params object[] args)
        {
            var method = o.GetType().GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            return (T)method?.Invoke(null, args);
        }
        
        public ref T FieldReference<T>(string fieldName)
        {
            var field = o.GetType().GetField(fieldName, 
                BindingFlags.Public | BindingFlags.NonPublic | 
                BindingFlags.Instance | BindingFlags.Static);
            
            if (field == null)
                throw new ArgumentException($"Field '{fieldName}' not found on type '{o.GetType()}'");
            
            if (field.FieldType != typeof(T))
                throw new ArgumentException($"Field type mismatch: expected {typeof(T)}, got {field.FieldType}");

            // 动态方法签名：ref T Method(object instance)
            var method = new DynamicMethod(
                name: $"FieldRef_{o.GetType().Name}_{fieldName}",
                returnType: typeof(T).MakeByRefType(),
                parameterTypes: field.IsStatic ? Type.EmptyTypes : [typeof(object)],
                owner: o.GetType(),
                skipVisibility: true  // 允许访问 private 字段
            );

            var il = method.GetILGenerator();

            if (field.IsStatic)
            {
                // ldsflda：加载静态字段地址
                il.Emit(OpCodes.Ldsflda, field);
            }
            else
            {
                // ldarg.0：加载 instance
                il.Emit(OpCodes.Ldarg_0);
            
                // 值类型需要 unbox，引用类型需要 castclass
                if (o.GetType().IsValueType)
                    il.Emit(OpCodes.Unbox, o.GetType());
                else
                    il.Emit(OpCodes.Castclass, o.GetType());
                
                // ldflda：加载实例字段地址
                il.Emit(OpCodes.Ldflda, field);
            }

            il.Emit(OpCodes.Ret);

            if (field.IsStatic)
            {
                var staticDelegate = (StaticFieldRef<T>)method.CreateDelegate(typeof(StaticFieldRef<T>));
                return ref staticDelegate();
            }
            else
            {
                var instanceDelegate = (InstanceFieldRef<T>)method.CreateDelegate(typeof(InstanceFieldRef<T>));
                return ref instanceDelegate(o);
            }
        }
        
        public static MethodInfo[] GetMethodsWithAttributes<T>(Type type) where T : Attribute
        {
            return type
                .GetMethods(BindingFlags.Public | BindingFlags.Instance |BindingFlags.Static | BindingFlags.NonPublic)
                .Where(m => m.GetCustomAttributes<T>().Any())
                .ToArray();
        }
        public MethodInfo[] GetMethodsWithAttributes<T>() where T : Attribute
        {
            return o.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance |BindingFlags.Static | BindingFlags.NonPublic)
                .Where(m => m.GetCustomAttributes<T>().Any())
                .ToArray();
        }
    }
    private delegate ref T StaticFieldRef<T>();
    private delegate ref T InstanceFieldRef<T>(object instance);

    extension(MethodInfo method)
    {
        public T CreateDelegate<T>(object? instance = null) where T : Delegate
        {
            if (method.IsStatic)
                return (T)method.CreateDelegate(typeof(T));
            return (T)method.CreateDelegate(typeof(T), instance);
        }
    }
}