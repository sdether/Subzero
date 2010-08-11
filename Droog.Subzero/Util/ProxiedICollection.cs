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
using Castle.DynamicProxy;

namespace Droog.Subzero.Util {
    public class ProxiedICollection<T> : ICollection<T> {
        private readonly string _propertyName;
        private readonly Type _parentType;
        private readonly ICollection<T> _source;
        private readonly ProxyGenerator _generator;
        private readonly Frozen _frozen;
        private readonly List<T> _proxies;
        private readonly bool _usesProxies;

        public ProxiedICollection(string propertyName, Type parentType, IList<T> source, ProxyGenerator generator, Frozen frozen) {
            _propertyName = propertyName;
            _parentType = parentType;
            _source = source;
            _generator = generator;
            _frozen = frozen;

            // TODO: assumes that no
            if(!typeof(T).IsProxiableType()) {
                return;
            }
            _usesProxies = true;
            _proxies = new List<T>();
            foreach(var value in source) {
                _proxies.Add(Wrap(value));
            }
        }

        private T Wrap(T value) {
            return (T)value.Wrap(_generator, typeof(T), _frozen);
        }

        private ICollection<T> Target { get { return _usesProxies ? _proxies : _source; } }

        public IEnumerator<T> GetEnumerator() {
            return Target.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Add(T item) {
            ThrowIfFrozen();
            _source.Add(item);
            if(_usesProxies) {
                _proxies.Add(Wrap(item));
            }
        }

        private void ThrowIfFrozen() {
            if(_frozen.State) {
                throw new FrozenAccessException(string.Format("Cannot modify collection '{0}' on instance of '{1}'", _propertyName, _parentType));
            }
        }

        public void Clear() {
            ThrowIfFrozen();
            _source.Clear();
            if(_usesProxies) {
                _proxies.Clear();
            }
        }

        public bool Contains(T item) {
            if(_usesProxies && item.IsA<Freezer.IFreezableWrapper>()) {
                return _proxies.Contains(item);
            }
            return _source.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            _source.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item) {
            ThrowIfFrozen();
            if(!_usesProxies) {
                return _source.Remove(item);
            }
            if(!_source.Contains(item)) {
                return false;
            }
            var index = 0;
            foreach(var item2 in _source) {
                if(item.Equals(item2)) {
                    break;
                }
                index++;
            }
            _source.Remove(item);
            _proxies.RemoveAt(index);
            return true;
        }

        public int Count {
            get { return _source.Count; }
        }

        public bool IsReadOnly {
            get { return _source.IsReadOnly; }
        }
    }
}