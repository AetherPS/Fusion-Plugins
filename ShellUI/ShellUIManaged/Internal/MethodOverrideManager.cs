using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fusion.Internal
{
    public static class MethodOverrideManager
    {
        [DllImport("ShellUI.sprx", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern IntPtr AddMethodDetour(string hookKey, IntPtr from, IntPtr to);

        public static void Initialize()
        {
            var assembly = Assembly.GetExecutingAssembly();
            foreach (var method in assembly.GetTypes().SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)))
            {
                var attribute = method.GetCustomAttribute<MethodOverrideAttribute>();
                if (attribute == null)
                {
                    continue;
                }

                var targetName = attribute.TargetMethodName ?? method.Name;
                var methodParameters = method.GetParameters();

                // Determine if this is an instance method (first param is the instance).
                var isInstanceMethod = methodParameters.Length > 0 && methodParameters[0].ParameterType == attribute.TargetType;

                // Get effective parameter types (excluding instance parameter if present).
                var effectiveParamTypes = (isInstanceMethod ? methodParameters.Skip(1) : methodParameters)
                    .Select(p => p.ParameterType)
                    .ToArray();

                var targetMethod = (targetName == ".ctor" || targetName == ".cctor")
                    ? FindConstructor(attribute.TargetType, targetName, effectiveParamTypes)
                    : FindMethod(attribute.TargetType, targetName, effectiveParamTypes, isInstanceMethod);

                if (targetMethod == null)
                {
                    throw new MissingMethodException($"Target method not found {attribute.TargetType}.{targetName}({string.Join(", ", effectiveParamTypes.Select(t => t.Name))})");
                }

                RuntimeHelpers.RunClassConstructor(attribute.TargetType.TypeHandle);
                RuntimeHelpers.RunClassConstructor(method.DeclaringType.TypeHandle);

                RuntimeHelpers.PrepareMethod(targetMethod.MethodHandle);
                RuntimeHelpers.PrepareMethod(method.MethodHandle);

                var stubPointer = AddMethodDetour($"{attribute.TargetType}.{targetName}", targetMethod.MethodHandle.Value, method.MethodHandle.Value);

                if (stubPointer == IntPtr.Zero)
                {
                    throw new InvalidOperationException($"Failed to create method detour for {attribute.TargetType}.{targetName}");
                }

                // Auto-detect stub field name if not specified
                var stubFieldName = attribute.DelegateFieldName;
                if (string.IsNullOrEmpty(stubFieldName))
                {
                    // Generate default field name: _MethodName_stub
                    stubFieldName = $"_{method.Name}_stub";
                }

                var stubField = method.DeclaringType.GetField(stubFieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (stubField == null)
                {
                    Console.WriteLine($"Warning: Stub field '{stubFieldName}' not found in '{method.DeclaringType.FullName}'");
                }
                else if (stubField.FieldType != typeof(IntPtr))
                {
                    throw new InvalidOperationException($"Stub field '{stubFieldName}' must be of type IntPtr, got {stubField.FieldType.Name}");
                }
                else
                {
                    stubField.SetValue(null, stubPointer);
                }
            }
        }

        private static MethodBase FindConstructor(Type targetType, string ctorName, Type[] paramTypes)
        {
            if (ctorName == ".cctor")
                return targetType.TypeInitializer;

            return targetType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(c => ParametersMatch(c.GetParameters(), paramTypes));
        }

        private static MethodInfo FindMethod(Type targetType, string methodName, Type[] paramTypes, bool isInstance)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic;
            flags |= isInstance ? BindingFlags.Instance : BindingFlags.Static;

            var declaredMethods = targetType.GetMethods(flags | BindingFlags.DeclaredOnly);

            var declaredMatch = declaredMethods
                .FirstOrDefault(m => m.Name == methodName && ParametersMatch(m.GetParameters(), paramTypes));

            if (declaredMatch != null)
                return declaredMatch;

            var inheritedMatch = targetType.GetMethods(flags)
                .FirstOrDefault(m => m.Name == methodName && ParametersMatch(m.GetParameters(), paramTypes));

            if (inheritedMatch != null)
                return inheritedMatch;

            Console.WriteLine($"[MethodOverride] *** METHOD NOT FOUND ***");
            Console.WriteLine($"[MethodOverride] Searched for: {methodName} with {paramTypes.Length} parameters");
            Console.WriteLine($"[MethodOverride] Instance method: {isInstance}");
            Console.WriteLine($"[MethodOverride] Available methods in {targetType.FullName}:");
            foreach (var method in targetType.GetMethods(flags).Where(m => m.Name == methodName))
            {
                Console.WriteLine($"[MethodOverride]   - {method.Name}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))}) - IsStatic: {method.IsStatic}");
            }

            return null;
        }

        private static bool ParametersMatch(ParameterInfo[] actual, Type[] expected)
        {
            return actual.Length == expected.Length &&
                   actual.Zip(expected, (a, e) => a.ParameterType == e).All(match => match);
        }
    }
}