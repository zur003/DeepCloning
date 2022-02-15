﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace ShereSoft.SpecializedCloners
{
    static class GenericListCloner
    {
        public static bool CanMap(object value)
        {
            var type = value.GetType();
            return type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(List<>) || type.GetGenericTypeDefinition() == typeof(HashSet<>));
        }

        public static CloneObjectDelegate<T> Buid<T>()
        {
            var type = typeof(T);
            var method = new DynamicMethod(String.Empty, type, new Type[] { type, typeof(Dictionary<object, object>), typeof(DeepCloningOptions) });
            var il = method.GetILGenerator();

            var lblRepeat = il.DefineLabel();
            var lblMoveNext = il.DefineLabel();
            var t = type.GenericTypeArguments[0];

            il.DeclareLocal(type);
            il.DeclareLocal(typeof(IEnumerator<>).MakeGenericType(t));
            il.DeclareLocal(t);
            il.DeclareLocal(typeof(bool));

            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Call, typeof(DeepCloningOptions).GetProperty(nameof(DeepCloningOptions.None.DeepCloneStrings)).GetMethod);
            il.Emit(OpCodes.Stloc_3);

            il.Emit(OpCodes.Ldarg_0);

            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Call, type.GetProperty("Count").GetMethod);
            il.Emit(OpCodes.Newobj, type.GetConstructor(new[] { typeof(int) }));
            il.Emit(OpCodes.Stloc_0);

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Callvirt, typeof(Dictionary<object, object>).GetMethod("Add"));

            il.Emit(OpCodes.Callvirt, typeof(IEnumerable<>).MakeGenericType(t).GetMethod("GetEnumerator"));
            il.Emit(OpCodes.Stloc_1);
            il.Emit(OpCodes.Br_S, lblMoveNext);
            il.MarkLabel(lblRepeat);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Callvirt, typeof(IEnumerator<>).MakeGenericType(t).GetProperty("Current").GetMethod);
            il.Emit(OpCodes.Stloc_2);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldloc_2);

            var add = type.GetMethod("Add");

            if (t == typeof(string))
            {
                il.Emit(OpCodes.Ldloc_3);
                var skipDeepCloneString = il.DefineLabel();
                il.Emit(OpCodes.Brfalse, skipDeepCloneString);
                il.Emit(OpCodes.Callvirt, typeof(string).GetMethod(nameof(String.Empty.ToCharArray), Type.EmptyTypes));
                il.Emit(OpCodes.Newobj, typeof(string).GetConstructor(new[] { typeof(char[]) }));
                il.MarkLabel(skipDeepCloneString);
                il.Emit(OpCodes.Call, add);
            }
            else if (t.IsValueType)
            {
                if (!DeepCloning.IsSimpleType(t))
                {
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldarg_2);
                    il.Emit(OpCodes.Call, typeof(DeepCloning<>).MakeGenericType(t).GetMethod(nameof(DeepCloning<T>.DeepCloneStruct), BindingFlags.NonPublic | BindingFlags.Static));
                }

                il.Emit(OpCodes.Call, add);
            }
            else
            {
                il.Emit(OpCodes.Dup);
                var lblSkipSetIfNull = il.DefineLabel();
                il.Emit(OpCodes.Brfalse, lblSkipSetIfNull);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Call, typeof(DeepCloning<>).MakeGenericType(t).GetMethod(nameof(DeepCloning<T>.DeepCloneObject), BindingFlags.NonPublic | BindingFlags.Static));
                il.Emit(OpCodes.Call, add);
                var lblAvoidPopIfNotNull = il.DefineLabel();
                il.Emit(OpCodes.Br, lblAvoidPopIfNotNull);

                il.MarkLabel(lblSkipSetIfNull);
                il.Emit(OpCodes.Pop);  // pop null value
                il.Emit(OpCodes.Pop);  // pop dest ref

                il.MarkLabel(lblAvoidPopIfNotNull);
            }

            if (add.ReturnType != typeof(void))
            {
                il.Emit(OpCodes.Pop);
            }

            il.MarkLabel(lblMoveNext);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Callvirt, typeof(IEnumerator).GetMethod("MoveNext"));
            il.Emit(OpCodes.Brtrue_S, lblRepeat);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

#if NET45 || NETCOREAPP
            return (CloneObjectDelegate<T>)method.CreateDelegate(typeof(CloneObjectDelegate<T>));
#else
            return method.CreateDelegate<CloneObjectDelegate<T>>();
#endif
        }
    }
}