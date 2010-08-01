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
using Droog.Subzero.Util;
using NUnit.Framework;

namespace Droog.Subzero.Test {

    [TestFixture]
    public class IFreezableWithoutCloneTests {

        [Test]
        public void Can_call_methods_on_wrapped_instance() {
            var data = new FreezableData() { Id = 42, Name = "Everything" };
            var data2 = Freezer.AsFreezable(data);
            Assert.IsTrue(data2.IsA<Freezer.IFreezableWrapper>());
            AssertSameValue(data, data2);
        }

        [Test]
        public void Wrapped_instance_is_unfrozen_by_default() {
            Assert.IsFalse(CreateData().IsFrozen);
        }

        [Test]
        public void Can_freeze_instance() {
            var data = CreateData();
            data.Freeze();
            Assert.IsTrue(data.IsFrozen);
        }

        [Test]
        public void Thaw_on_unfrozen_returns_new_unfrozen_instance() {
            var data = CreateData();
            var data2 = data.Thaw();
            Assert.AreNotSame(data, data2);
            AssertSameValue(data, data2);
            Assert.IsFalse(data2.IsFrozen);
        }

        [Test]
        public void Thaw_on_frozen_returns_new_unfrozen_instance() {
            var data = CreateData();
            data.Freeze();
            var data2 = data.Thaw();
            Assert.AreNotSame(data, data2);
            AssertSameValue(data, data2);
            Assert.IsFalse(data2.IsFrozen);
        }

        [Test]
        [ExpectedException(typeof(FrozenAccessException))]
        public void Setting_off_frozen_instance_property_throws() {
            var data = CreateData();
            data.Freeze();
            data.Id = 45;
        }

        [Test]
        public void Can_read_frozen_property() {
            var data = CreateData();
            data.Freeze();
            var id = data.Id;
        }


        [Test]
        public void FreezeDry_clones_unfrozen_instance() {
            var data = CreateData();
            var data2 = data.FreezeDry();
            AssertSameValue(data, data2);
            Assert.IsFalse(data.IsFrozen);
            Assert.IsTrue(data2.IsFrozen);
        }

        [Test]
        public void FreezeDry_on_frozen_instance_is_noop() {
            var data = CreateData();
            data.Freeze();
            var data2 = data.FreezeDry();
            Assert.AreSame(data, data2);
            Assert.IsTrue(data2.IsFrozen);
        }

        private void AssertSameValue(FreezableData first, FreezableData next) {
            Assert.AreEqual(first.Id, next.Id);
            Assert.AreEqual(first.Name, next.Name);
        }

        private FreezableData CreateData() {
            return Freezer.AsFreezable(new FreezableData { Id = 42, Name = "Everything" });
        }

        public class FreezableData : IFreezable<FreezableData> {

            public virtual int Id { get; set; }
            public virtual string Name { get; set; }

            #region Implementation of IFreezable<Data>
            public virtual void Freeze() { throw new NotImplementedException(); }
            public virtual FreezableData FreezeDry() { throw new NotImplementedException(); }

            public virtual bool IsFrozen { get { return false; } }
            public virtual FreezableData Thaw() { throw new NotImplementedException(); }
            #endregion
        }

    }
}
