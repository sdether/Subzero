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
using Droog.Subzero.Util;
using NUnit.Framework;

namespace Droog.Subzero.Test {

    [TestFixture]
    public class PocoTests {

        [Test]
        public void Can_call_methods_on_wrapped_instance() {
            var data = new Data() { Id = 42, Name = "Everything" };
            var data2 = Freezer.AsFreezable(data);
            Assert.IsTrue(data2.IsA<Freezer.IFreezableWrapper>());
            AssertSameValue(data, data2);
        }

        [Test]
        public void Can_detect_wrapped_instances() {
            var data = new Data() { Id = 42, Name = "Everything" };
            Assert.IsFalse(Freezer.IsFreezable(data));
            var data2 = Freezer.AsFreezable(data);
            Assert.IsTrue(Freezer.IsFreezable(data2));
        }

        [Test]
        [ExpectedException(typeof(NonFreezableException))]
        public void Freeze_throws_on_non_freezable() {
            var data = new Data() { Id = 42, Name = "Everything" };
            Freezer.Freeze(data);
        }

        [Test]
        [ExpectedException(typeof(NonFreezableException))]
        public void IsFrozen_throws_on_non_freezable() {
            var data = new Data() { Id = 42, Name = "Everything" };
            Freezer.IsFrozen(data);
        }

        [Test]
        [ExpectedException(typeof(NonFreezableException))]
        public void Thaw_throws_on_non_freezable() {
            var data = new Data() { Id = 42, Name = "Everything" };
            Freezer.Thaw(data);
        }

        [Test]
        public void Wrapped_instance_is_unfrozen_by_default() {
            Assert.IsFalse(Freezer.IsFrozen(CreateData()));
        }

        [Test]
        public void Can_freeze_instance() {
            var data = CreateData();
            Freezer.Freeze(data);
            Assert.IsTrue(Freezer.IsFrozen(data));
        }

        [Test]
        public void Thaw_on_unfrozen_returns_new_unfrozen_instance() {
            var data = CreateData();
            var data2 = Freezer.Thaw(data);
            Assert.AreNotSame(data, data2);
            AssertSameValue(data, data2);
            Assert.IsFalse(Freezer.IsFrozen(data2));
        }

        [Test]
        public void Thaw_on_frozen_returns_new_unfrozen_instance() {
            var data = CreateData();
            Freezer.Freeze(data);
            var data2 = Freezer.Thaw(data);
            Assert.AreNotSame(data, data2);
            AssertSameValue(data, data2);
            Assert.IsFalse(Freezer.IsFrozen(data2));
        }

        [Test]
        [ExpectedException(typeof(FrozenAccessException))]
        public void Setting_off_frozen_instance_property_throws() {
            var data = CreateData();
            Freezer.Freeze(data);
            data.Id = 45;
        }

        [Test]
        public void Can_read_frozen_property() {
            var data = CreateData();
            Freezer.Freeze(data);
            var id = data.Id;
        }

        [Test]
        public void FreezeDry_on_non_freezable_returns_freezable() {
            var data = new Data() { Id = 42, Name = "Everything" };
            var data2 = Freezer.FreezeDry(data);
            Assert.AreNotSame(data,data2);
            Assert.IsTrue(Freezer.IsFreezable(data2));
            AssertSameValue(data, data2);
            Assert.IsTrue(Freezer.IsFrozen(data2));
        }

        [Test]
        public void FreezeDry_clones_unfrozen_instance() {
            var data = CreateData();
            var data2 = Freezer.FreezeDry(data);
            AssertSameValue(data, data2);
            Assert.IsFalse(Freezer.IsFrozen(data));
            Assert.IsTrue(Freezer.IsFrozen(data2));
        }

        [Test]
        public void FreezeDry_on_frozen_instance_is_noop() {
            var data = CreateData();
            Freezer.Freeze(data);
            var data2 = Freezer.FreezeDry(data);
            Assert.AreSame(data, data2);
            Assert.IsTrue(Freezer.IsFrozen(data2));
        }

        private void AssertSameValue(Data first, Data next) {
            Assert.AreEqual(first.Id, next.Id);
            Assert.AreEqual(first.Name, next.Name);
        }

        private Data CreateData() {
            return Freezer.AsFreezable(new Data { Id = 42, Name = "Everything" });
        }

        public class Data {
            public virtual int Id { get; set; }
            public virtual string Name { get; set; }
        }

        public class DataGraph {
            public DataGraph() {
                ReadonlyData = new Data();
            }
            public virtual Data SettableData { get; set; }
            public virtual Data ReadonlyData { get; private set; }
        }
    }
}
