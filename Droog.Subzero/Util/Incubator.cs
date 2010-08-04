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
using System.Collections;
using System.Reflection;

namespace Droog.Subzero.Util {
    public class Incubator {

        private Incubator() { }

        public static T Clone<T>(T dto) where T : class {
            return new Incubator().DeepCopy(dto);
        }

        private T DeepCopy<T>(T dto) {

            return (T)Copy(dto, dto.GetType());
        }

        private object Copy(object o, Type t) {
            //var t = o.GetType();
            if(t.IsValueType || o is string) {
                return o;
            }
            Func<object, object> copier;
            if(o is IEnumerable) {
                copier = GetEnumerableCopier(t);
            } else {
                copier = CopyObject;
            }
            return copier(o);
        }

        private Func<object, object> GetEnumerableCopier(Type enumerableType) {
            // check for supported type, should be only Array, ArrayList, ICollection<>, IList<> and IDictionary<>

            if(enumerableType.IsArray) {
                return CopyArray;
            }
            if(!enumerableType.IsGenericType) {
                if(enumerableType.IsA<ArrayList>()) {
                    return CopyArrayList;
                }
            } else {
                return CopyAsArray;
            }
            throw new ArgumentException(string.Format("Type '{0}' not supported for cloning", enumerableType));
        }

        private object CopyArrayList(object enumerable) {
            var copy = new ArrayList();
            foreach(var item in (IEnumerable)enumerable) {
                copy.Add(Copy(item,item.GetType()));
            }
            return copy;
        }

        private object CopyArray(object enumerable) {
            var arrayType = enumerable.GetType();
            var itemType = arrayType.GetElementType();
            var length = (int)arrayType.GetProperty("Length").GetGetMethod().Invoke(enumerable, null);
            var copy = Array.CreateInstance(itemType, length);
            var i = 0;
            foreach(var item in (IEnumerable)enumerable) {
                copy.SetValue(Copy(item, itemType), i);
                i++;
            }
            return copy;
        }

        private object CopyAsArray(object enumerable) {
            var enumerableType = enumerable.GetType();
            var itemType = enumerableType.IsArray
                ? enumerableType.GetElementType()
                : enumerableType.IsGenericType ? enumerableType.GetGenericArguments()[0] : typeof (object);
            var list = new ArrayList();
            foreach(var item in (IEnumerable)enumerable) {
                list.Add(Copy(item, itemType));
            }
            var copy = Array.CreateInstance(itemType, list.Count);
            list.CopyTo(copy);
            return copy;
        }

        private object CopyObject(object o) {
            var t = o.GetType();
            var copy = Activator.CreateInstance(t, true);
            foreach(var property in t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)) {
                var value = property.GetValue(o, null);
                if(value == null) {
                    continue;
                }
                property.SetValue(copy, Copy(value, property.PropertyType), null);
            }
            return copy;
        }
    }
}
