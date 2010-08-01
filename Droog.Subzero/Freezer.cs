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
using System.Reflection;
using Castle.Core.Interceptor;
using Castle.DynamicProxy;
using Droog.Subzero.Util;
using Metsys.Little;

namespace Droog.Subzero {

    public class FrozenAccessException : Exception {
    }

    public class Freezer {

        public interface IFreezableWrapper { }

        private readonly ProxyGenerator _generator = new ProxyGenerator();

        // TODO: need to make sure all members are virtual
        // TODO: combine Build and AsFreezable?
        public T Build<T>(T instance) where T : class, IFreezable<T> {
            if(instance.IsA<IFreezableWrapper>()) {
                return instance;
            }
            return (T)_generator.CreateClassProxy(typeof(T), new[] { typeof(IFreezableWrapper) }, new FreezableInterceptor<T>(_generator, instance));
        }

        public T AsFreezable<T>(T instance) where T : class {
            if(instance.IsA<IFreezableWrapper>()) {
                return instance;
            }
            if(instance.IsA<IFreezable<T>>()) {
                return (T)_generator.CreateClassProxy(typeof(T), new[] { typeof(IFreezableWrapper) }, new FreezableInterceptor<T>(_generator, instance));
            }
            return (T)_generator.CreateClassProxy(typeof(T), new[] { typeof(IFreezable<T>), typeof(IFreezableWrapper) }, new FreezableInterceptor<T>(_generator, instance));
        }

        private class FreezableInterceptor<T> : IInterceptor where T : class {
            private readonly ProxyGenerator _generator;
            private readonly object _instance;
            private readonly MethodInfo _cloneMethod;
            private bool _frozen;

            public FreezableInterceptor(ProxyGenerator generator, object instance) {
                _generator = generator;
                _instance = instance;
                var cloneMethod = _instance.GetType().GetMethod("Clone", BindingFlags.Public | BindingFlags.Instance);
                if(cloneMethod != null && cloneMethod.ReturnType == _instance.GetType() && !cloneMethod.IsGenericMethod && cloneMethod.GetParameters().Length == 0) {
                    _cloneMethod = cloneMethod;
                }
            }

            public void Intercept(IInvocation invocation) {
                switch(invocation.MethodInvocationTarget.Name) {
                    case "Freeze":
                        _frozen = true;
                        return;
                    case "get_IsFrozen":
                        invocation.ReturnValue = _frozen;
                        return;
                    case "Thaw":
                        T thawed;
                        if(_cloneMethod != null) {
                            thawed = (T)_cloneMethod.Invoke(_instance,new object[0]);
                        } else {
                            thawed = Deserializer.Deserialize<T>(Serializer.Serialize(_instance));
                        }
                        invocation.ReturnValue = Wrap(thawed);
                        return;
                    case "Clone":
                        var instance = invocation.MethodInvocationTarget.Invoke(_instance, invocation.Arguments);
                        invocation.ReturnValue = Wrap(instance);
                        return;
                }
                invocation.ReturnValue = invocation.MethodInvocationTarget.Invoke(_instance, invocation.Arguments);
            }

            private object Wrap(object instance) {
                return _generator.CreateClassProxy(typeof(T), new[] { typeof(IFreezable<T>), typeof(IFreezableWrapper) }, new FreezableInterceptor<T>(_generator, instance));
            }
        }
    }
}
