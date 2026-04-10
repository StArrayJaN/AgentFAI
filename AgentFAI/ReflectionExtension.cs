using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace AgentFAI;

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
            return ref AccessTools.FieldRefAccess<T>(o.GetType(), fieldName)(o);
        }
        public ref T StaticFieldReference<T>(string fieldName)
        {
            return ref AccessTools.StaticFieldRefAccess<T>(o.GetType(), fieldName);
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
}