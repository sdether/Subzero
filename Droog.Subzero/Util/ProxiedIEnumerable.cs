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
using System.Collections;
using System.Collections.Generic;
using Castle.DynamicProxy;

namespace Droog.Subzero.Util {
    public class ProxiedIEnumerable<T> : IEnumerable<T> {

        private readonly IEnumerable<T> _values;

        public ProxiedIEnumerable(IEnumerable<T> source, ProxyGenerator generator, Frozen frozen) {
            if(typeof(T).IsProxiableType()) {
                var list = new List<T>();
                _values = list;
                foreach(var value in source) {
                    list.Add((T)value.Wrap(generator, typeof(T), frozen));
                }
            } else {
                _values = source;
            }
        }

        public IEnumerator<T> GetEnumerator() {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}