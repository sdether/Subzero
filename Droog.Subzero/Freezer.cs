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
using System.Reflection;
using Castle.Core.Interceptor;
using Castle.DynamicProxy;
using Droog.Subzero.Util;
using Metsys.Little;

namespace Droog.Subzero {
    public class Freezer {

        public interface IFreezableWrapper { }

        private readonly ProxyGenerator _generator = new ProxyGenerator();

        // TODO: need to make sure all members are virtual
        public T AsFreezable<T>(T instance) where T : class {
            if(instance.IsA<IFreezableWrapper>()) {
                return instance;
            }
            return (T)Wrap(_generator, instance, false);
        }

        private static object Wrap<T>(ProxyGenerator generator, T instance, bool frozen) where T : class {
            var interceptor = new FreezableInterceptor<T>(generator, instance, frozen);
            var freezable = generator.CreateClassProxy(typeof(T), new[] { typeof(IFreezable<T>), typeof(IFreezableWrapper) }, interceptor);
            interceptor.This = freezable;
            return freezable;
        }

        private class FreezableInterceptor<T> : IInterceptor where T : class {
            private readonly ProxyGenerator _generator;
            private readonly object _instance;
            private readonly MethodInfo _cloneMethod;
            private bool _frozen;
            public object This;

            public FreezableInterceptor(ProxyGenerator generator, object instance, bool frozen) {
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
                        _frozen = true;
                        return;
                    case "get_IsFrozen":
                        invocation.ReturnValue = _frozen;
                        return;
                    case "FreezeDry":
                        if(_frozen) {
                            invocation.ReturnValue = This;
                            return;
                        }
                        invocation.ReturnValue = Clone(true);
                        return;
                    case "Thaw":
                        invocation.ReturnValue = Clone(false);
                        return;
                    case "Clone":
                        var instance = invocation.MethodInvocationTarget.Invoke(_instance, invocation.Arguments);
                        invocation.ReturnValue = Wrap(_generator, (T)instance, false);
                        return;
                }
                if(_frozen && methodName.StartsWith("set_")) {
                    throw new FrozenAccessException(string.Format("Cannot set '{0}' on frozen instance of '{1}'",methodName.Substring(4),_instance.GetType()));
                }
                invocation.ReturnValue = invocation.MethodInvocationTarget.Invoke(_instance, invocation.Arguments);
            }

            private object Clone(bool frozen) {
                T clone;
                if(_cloneMethod != null) {
                    clone = (T)_cloneMethod.Invoke(_instance, new object[0]);
                } else {
                    clone = Deserializer.Deserialize<T>(Serializer.Serialize(_instance));
                }
                return Wrap(_generator, clone, frozen);
            }
        }
    }
}
