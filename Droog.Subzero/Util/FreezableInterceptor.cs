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
    internal interface IFreezableInterceptor {
        void SetWrappedInstance(object wrapped);
    }

    internal class FreezableInterceptor<T> : IFreezableInterceptor, IInterceptor where T : class {
        private readonly ProxyGenerator _generator;
        private readonly T _instance;
        private readonly Dictionary<string, object> _properties = new Dictionary<string, object>();
        private readonly MethodInfo _cloneMethod;
        private readonly Frozen _frozen;
        private object _this;

        public FreezableInterceptor(ProxyGenerator generator, T instance, Frozen frozen) {
            _generator = generator;
            _instance = instance;
            _frozen = frozen;
            var cloneMethod = _instance.GetType().GetMethod("Clone", BindingFlags.Public | BindingFlags.Instance);
            if(cloneMethod != null && cloneMethod.ReturnType == _instance.GetType() && !cloneMethod.IsGenericMethod && cloneMethod.GetParameters().Length == 0) {
                _cloneMethod = cloneMethod;
            }
        }

        public void Intercept(IInvocation invocation) {
            var methodName = invocation.MethodInvocationTarget.Name;
            switch(methodName) {
                case "Freeze":
                    _frozen.State = true;
                    return;
                case "get_IsFrozen":
                    invocation.ReturnValue = _frozen.State;
                    return;
                case "FreezeDry":
                    if(_frozen.State) {
                        invocation.ReturnValue = _this;
                        return;
                    }
                    invocation.ReturnValue = Clone(Frozen.True);
                    return;
                case "Thaw":
                    invocation.ReturnValue = Clone(Frozen.False);
                    return;
                case "Clone":
                    var instance = (T)invocation.MethodInvocationTarget.Invoke(_instance, invocation.Arguments);
                    invocation.ReturnValue = instance.Wrap(_generator, Frozen.False);
                    return;
            }
            var propertyInfo = invocation.MethodInvocationTarget.GetPropertyMethodInfo();
            if(propertyInfo.IsSetter) {
                if(_frozen.State) {
                    throw new FrozenAccessException(string.Format("Cannot set '{0}' on frozen instance of '{1}'", methodName.Substring(4), _instance.GetType()));
                }
                var setValue = invocation.Arguments[0];
                if(setValue != null && !Freezer.IsFreezable(setValue)) {
                    if(propertyInfo.Type.IsGenericList()) {
                        _properties[methodName] = CreateProxiedList(methodName, propertyInfo.Type, setValue);
                    } else if(propertyInfo.Type.IsGenericCollection()) {
                        _properties[methodName] = CreateProxiedCollection(methodName, propertyInfo.Type, setValue);
                    } else if(propertyInfo.Type.IsGenericEnumerable()) {
                        _properties[methodName] = CreateProxiedEnumerable(propertyInfo.Type, setValue);
                    } else if(propertyInfo.Type.IsProxiableType()) {
                        _properties[methodName] = setValue.Wrap(_generator, propertyInfo.Type, _frozen);
                    }
                }
            } else if(propertyInfo.IsGetter && (propertyInfo.Type.IsGenericEnumerable() || propertyInfo.Type.IsProxiableType())) {
                object proxiedValue;
                if(_properties.TryGetValue(methodName, out proxiedValue)) {
                    invocation.ReturnValue = proxiedValue;
                    return;
                }
            }
            var returnValue = invocation.MethodInvocationTarget.Invoke(_instance, invocation.Arguments);
            if(returnValue != null && propertyInfo.IsGetter) {
                if(propertyInfo.Type.IsGenericList()) {
                    returnValue = CreateProxiedList(methodName, propertyInfo.Type, returnValue);
                } else if(propertyInfo.Type.IsGenericCollection()) {
                    returnValue = CreateProxiedCollection(methodName, propertyInfo.Type, returnValue);
                } else if(propertyInfo.Type.IsGenericEnumerable()) {
                    returnValue = CreateProxiedEnumerable(propertyInfo.Type, returnValue);
                } else if(propertyInfo.Type.IsProxiableType()) {
                    returnValue = returnValue.Wrap(_generator, invocation.MethodInvocationTarget.ReturnType, _frozen);
                }
                _properties[methodName] = returnValue;
            }
            invocation.ReturnValue = returnValue;
        }

        private object CreateProxiedEnumerable(Type type, object value) {
            var itemType = type.GetEnumerableItemType();
            var enumerableType = typeof(ProxiedIEnumerable<>).MakeGenericType(itemType);

            // TODO: create and cache fast ctor
            return Activator.CreateInstance(enumerableType, value, _generator, _frozen);
        }

        private object CreateProxiedCollection(string methodName, Type type, object value) {
            var itemType = type.GetEnumerableItemType();
            var enumerableType = typeof(ProxiedICollection<>).MakeGenericType(itemType);

            // TODO: create and cache fast ctor
            return Activator.CreateInstance(enumerableType, methodName, typeof(T), value, _generator, _frozen);
        }

        private object CreateProxiedList(string methodName, Type type, object value) {
            var itemType = type.GetEnumerableItemType();
            var enumerableType = typeof(ProxiedIList<>).MakeGenericType(itemType);

            // TODO: create and cache fast ctor
            return Activator.CreateInstance(enumerableType, methodName, typeof(T), value, _generator, _frozen);
        }

        private object Clone(Frozen frozen) {
            T clone;
            if(_cloneMethod != null) {
                clone = (T)_cloneMethod.Invoke(_instance, new object[0]);
            } else {
                //clone = Deserializer.Deserialize<T>(Serializer.Serialize(_instance));
                clone = Incubator.Clone(_instance, new[] { "IsFrozen" });
            }
            return clone.Wrap(_generator, frozen);
        }

        public void SetWrappedInstance(object wrapped) {
            _this = wrapped;
        }
    }
}
