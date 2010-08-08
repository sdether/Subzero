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
using System.Reflection;
using Castle.Core.Interceptor;
using Castle.DynamicProxy;
using Droog.Subzero.Util;

namespace Droog.Subzero {
    public class Freezer {

        public interface IFreezableWrapper { }

        private static readonly ProxyGenerator _generator = new ProxyGenerator();

        // TODO: need to check the type for virtuals and supported collection types
        public static T AsFreezable<T>(T instance) where T : class {
            if(instance.IsA<IFreezableWrapper>()) {
                return instance;
            }
            return (T)Wrap(_generator, instance, Frozen.False);
        }

        public static bool IsFreezable<T>(T instance) where T : class {
            return CheckInstance(instance);
        }

        public static bool IsFrozen<T>(T instance) where T : class {
            var freezable = CastToFreezableOrThrow(instance);
            return freezable.IsFrozen;
        }

        public static void Freeze<T>(T instance) where T : class {
            var freezable = CastToFreezableOrThrow(instance);
            freezable.Freeze();
        }

        public static T Thaw<T>(T instance) where T : class {
            var freezable = CastToFreezableOrThrow(instance);
            return freezable.Thaw();
        }

        public static T FreezeDry<T>(T instance) where T : class {
            var freezable = instance as IFreezable<T>;
            return freezable == null ? ((IFreezable<T>)AsFreezable(instance)).FreezeDry() : freezable.FreezeDry();
        }


        private static IFreezable<T> CastToFreezableOrThrow<T>(T instance) where T : class {
            if(!CheckInstance(instance)) {
                throw new NonFreezableException(string.Format("Instance '{0}' is not an IFreezable or wrapped by a freezable proxy", instance.GetType()));
            }
            return instance as IFreezable<T>;
        }

        private static bool CheckInstance(object instance) {
            return instance.IsA<IFreezableWrapper>();
        }

        private static object Wrap<T>(ProxyGenerator generator, T instance, Frozen frozen) where T : class {
            var interceptor = new FreezableInterceptor<T>(generator, instance, frozen);
            var freezable = generator.CreateClassProxy(typeof(T), new[] { typeof(IFreezable<T>), typeof(IFreezableWrapper) }, interceptor);
            interceptor.SetWrappedInstance(freezable);
            return freezable;
        }

        // TODO: should cache the constructors of these types
        private static object Wrap(ProxyGenerator generator, Type instanceType, object instance, Frozen frozen) {
            var interceptorType = typeof(FreezableInterceptor<>).MakeGenericType(instanceType);
            var interceptorCtor = interceptorType.GetConstructor(new[] { typeof(ProxyGenerator), instanceType, typeof(Frozen) });
            var interceptor = (IInterceptor)interceptorCtor.Invoke(new[] { _generator, instance, frozen });
            var freezableType = typeof(IFreezable<>).MakeGenericType(instanceType);
            var freezable = generator.CreateClassProxy(instanceType, new[] { freezableType, typeof(IFreezableWrapper) }, interceptor);
            ((IFreezableInterceptor)interceptor).SetWrappedInstance(freezable);
            return freezable;
        }

        private interface IFreezableInterceptor {
            void SetWrappedInstance(object wrapped);
        }

        private class Frozen {
            public static Frozen False { get { return new Frozen(); } }
            public static Frozen True { get { return new Frozen() { State = true }; } }

            public bool State;

            private Frozen() { }
        }

        // TODO: need to wrap entire graph at construction time
        private class FreezableInterceptor<T> : IFreezableInterceptor, IInterceptor where T : class {
            private readonly ProxyGenerator _generator;
            private readonly T _instance;
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
                        var instance = invocation.MethodInvocationTarget.Invoke(_instance, invocation.Arguments);
                        invocation.ReturnValue = Wrap(_generator, (T)instance, Frozen.False);
                        return;
                }
                if(methodName.StartsWith("set_")) {
                    if(_frozen.State) {
                        throw new FrozenAccessException(string.Format("Cannot set '{0}' on frozen instance of '{1}'", methodName.Substring(4), _instance.GetType()));
                    }
                    var setValue = invocation.Arguments[0];
                    if(setValue != null && !(setValue is string) && !invocation.MethodInvocationTarget.ReturnType.IsValueType && !IsFreezable(setValue)) {
                        invocation.Arguments[0] = Wrap(_generator, invocation.MethodInvocationTarget.ReturnType, setValue, _frozen);
                    }
                }
                var returnValue = invocation.MethodInvocationTarget.Invoke(_instance, invocation.Arguments);
                if(returnValue != null && methodName.StartsWith("get_") && !(returnValue is string) && !invocation.MethodInvocationTarget.ReturnType.IsValueType && !IsFreezable(returnValue)) {
                    returnValue = Wrap(_generator, invocation.MethodInvocationTarget.ReturnType, returnValue, _frozen);
                }
                invocation.ReturnValue = returnValue;
            }

            private object Clone(Frozen frozen) {
                T clone;
                if(_cloneMethod != null) {
                    clone = (T)_cloneMethod.Invoke(_instance, new object[0]);
                } else {
                    //clone = Deserializer.Deserialize<T>(Serializer.Serialize(_instance));
                    clone = Incubator.Clone(_instance, new[] { "IsFrozen" });
                }
                return Wrap(_generator, clone, frozen);
            }

            public void SetWrappedInstance(object wrapped) {
                _this = wrapped;
            }
        }
    }
}
