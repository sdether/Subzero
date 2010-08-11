/*
 * Subzero 
 * Copyright (C) 2010 Arne F. Claassen
 * http://www.claassen.net/geek/blog geekblog [at] claassen [dot] net
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections.Generic;
using System.Reflection;
using Castle.Core.Interceptor;
using Castle.DynamicProxy;

namespace Droog.Subzero.Util {
    public static class SubzeroExtensions {
        public static bool IsA<T>(this object instance) {
            return instance == null ? false : IsA<T>(instance.GetType());
        }

        public static bool IsA(this Type type, Type target) {
            return type != null && target.IsAssignableFrom(type);
        }

        public static bool IsA<T>(this Type type) {
            if(type == null) {
                return false;
            }
            var t = typeof(T);
            return t.IsAssignableFrom(type);
        }

        internal static TypeInfo GetTypeInfo(this object o) {
            return TypeInfo.GetTypeInfo(o.GetType());
        }

        internal static Type GetEnumerableItemType(this Type t) {
            return t.IsArray
                ? t.GetElementType()
                : t.IsGenericType ? t.GetGenericArguments()[0] : typeof(object);
        }

        internal static bool IsProxiableType(this Type type) {
            return type != typeof(string) && !type.IsValueType;
        }

        internal static bool IsGenericList(this Type type) {
            return type.IsGenericType && type.GetGenericTypeDefinition().IsA(typeof(IList<>));
        }

        internal static bool IsGenericCollection(this Type type) {
            return type.IsGenericType && type.GetGenericTypeDefinition().IsA(typeof(ICollection<>));
        }

        internal static bool IsGenericEnumerable(this Type type) {
            return type.IsGenericType && type.GetGenericTypeDefinition().IsA(typeof(IEnumerable<>));
        }

        // TODO: should cache the constructors of these types
        internal static object Wrap(this object instance, ProxyGenerator generator, Type instanceType, Frozen frozen) {
            var interceptorType = typeof(FreezableInterceptor<>).MakeGenericType(instanceType);
            var interceptorCtor = interceptorType.GetConstructor(new[] { typeof(ProxyGenerator), instanceType, typeof(Frozen) });
            var interceptor = (IInterceptor)interceptorCtor.Invoke(new[] { generator, instance, frozen });
            var freezableType = typeof(IFreezable<>).MakeGenericType(instanceType);
            var freezable = generator.CreateClassProxy(instanceType, new[] { freezableType, typeof(Freezer.IFreezableWrapper) }, interceptor);
            ((IFreezableInterceptor)interceptor).SetWrappedInstance(freezable);
            return freezable;
        }

        internal static T Wrap<T>(this T instance, ProxyGenerator generator, Frozen frozen) where T : class {
            var interceptor = new FreezableInterceptor<T>(generator, instance, frozen);
            var freezable = generator.CreateClassProxy(typeof(T), new[] { typeof(IFreezable<T>), typeof(Freezer.IFreezableWrapper) }, interceptor);
            interceptor.SetWrappedInstance(freezable);
            return (T)freezable;
        }

        internal static PropertyMethodInfo GetPropertyMethodInfo(this MethodInfo methodInfo) {
            return new PropertyMethodInfo(methodInfo);
        }
    }
    public class PropertyMethodInfo {
        public readonly bool IsGetter;
        public readonly bool IsSetter;
        public readonly Type Type;
        public PropertyMethodInfo(MethodInfo methodInfo) {
            if(methodInfo.Name.StartsWith("set_")) {
                IsSetter = true;
                Type = methodInfo.GetParameters()[0].ParameterType;
            } else if(methodInfo.Name.StartsWith("get_")) {
                IsGetter = true;
                Type = methodInfo.ReturnType;
            }
        }
    }
}