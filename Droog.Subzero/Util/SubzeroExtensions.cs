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

namespace Droog.Subzero.Util {
    public static class SubzeroExtensions {
        public static bool IsA<T>(this object instance) {
            return instance == null ? false : IsA<T>(instance.GetType());
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
    }
}