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
using System.Collections.Generic;

namespace Droog.Subzero.Util {

    public class Incubator {
        private readonly HashSet<string> _ignore;

        private Incubator(string[] ignore) {
            _ignore = new HashSet<string>(ignore ?? new string[0]);
        }

        public static T Clone<T>(T dto) where T : class {
            return Clone(dto, null);
        }

        public static T Clone<T>(T dto, string[] ignore) where T : class {
            return new Incubator(ignore).DeepCopy(dto);
        }

        private T DeepCopy<T>(T dto) {
            return (T)Copy(dto, dto.GetType());
        }

        private object Copy(object o, Type t) {
            if(t.IsValueType || o is string) {
                return o;
            }
            Func<object, object> copier;
            if(o is IEnumerable) {
                if(t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IDictionary<,>)) {
                    copier = GetDictionaryCopier(t);
                } else {
                    copier = GetEnumerableCopier(t);
                }
            } else {
                copier = CopyObject;
            }
            return copier(o);
        }

        private Func<object, object> GetDictionaryCopier(Type type) {
            return dictionary => CopyDictionary(type, dictionary);
        }

        private object CopyDictionary(Type sourceDictionaryType, object dictionary) {
            var baseDictionaryType = typeof(Dictionary<,>);
            var basePairType = typeof(KeyValuePair<,>);
            var genericArgs = sourceDictionaryType.GetGenericArguments();
            var keyType = genericArgs[0];
            var valueType = genericArgs[1];
            var dictionaryType = baseDictionaryType.MakeGenericType(genericArgs);
            var pairType = basePairType.MakeGenericType(genericArgs);
            var clone = Activator.CreateInstance(dictionaryType);
            var addMethod = dictionaryType.GetMethod("Add");
            var keyGetter = pairType.GetProperty("Key").GetGetMethod();
            var valueGetter = pairType.GetProperty("Value").GetGetMethod();
            foreach(var pair in (IEnumerable)dictionary) {
                var key = keyGetter.Invoke(pair, null);
                var value = valueGetter.Invoke(pair, null);
                addMethod.Invoke(clone, new[] { Copy(key, keyType), Copy(value, valueType) });
            }
            return clone;
        }

        private Func<object, object> GetEnumerableCopier(Type enumerableType) {
            //TODO: check for supported type, should be only Array, ArrayList, ICollection<>, IList<> and IDictionary<>

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
                copy.Add(Copy(item, item.GetType()));
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
            var itemType = enumerableType.GetEnumerableItemType();
            var list = new ArrayList();
            foreach(var item in (IEnumerable)enumerable) {
                list.Add(Copy(item, itemType));
            }
            var copy = Array.CreateInstance(itemType, list.Count);
            list.CopyTo(copy);
            return copy;
        }

        private object CopyObject(object o) {
            var t = o.GetTypeInfo();
            var copy = t.Create();
            foreach(var property in t.Properties) {
                if(_ignore.Contains(property.Name)) {
                    continue;
                }
                var value = property.Get(o);
                if(value == null) {
                    continue;
                }
                property.Set(copy, Copy(value, property.Type));
            }
            return copy;
        }
    }
}
