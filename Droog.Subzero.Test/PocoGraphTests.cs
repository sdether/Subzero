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
using NUnit.Framework;

namespace Droog.Subzero.Test {

    [TestFixture]
    public class PocoGraphTests {

        [Test]
        public void Child_objects_are_wrapped() {
            var data = CreateData();
            Assert.IsTrue(Freezer.IsFreezable(data.Readonly));
        }

        [Test]
        public void Can_access_child_property() {
            var data = new DataGraph {Readonly = {Id = 42, Name = "Everything"}};
            var freezable = Freezer.AsFreezable(data);
            AssertSameValue(data,freezable);
        }

        [Test]
        public void Proxied_child_is_constant_instance() {
            var data = CreateData();
            Assert.AreSame(data.Readonly,data.Readonly);
        }

        [Test]
        public void Setting_property_wraps_the_instance_on_access() {
            var data = CreateData();
            data.Settable = new DataChild() {Id = 123, Name = "foo"};
            Assert.IsTrue(Freezer.IsFreezable(data.Settable));
        }

        [Test]
        public void Freezing_datagraph_causes_throw_on_child_set_of_readonly_child() {
            var data = CreateData();
            Freezer.Freeze(data);
            var child = data.Readonly;
            try {
                child.Id = 123;
            } catch(FrozenAccessException) {
                return;
            }
            Assert.Fail("shouldn't have been able to set child property");
        }


        [Test]
        public void Freezing_datagraph_causes_throw_on_child_set_of_settable_child() {
            var data = CreateData();
            data.Settable = new DataChild() { Id = 123, Name = "foo" };
            Freezer.Freeze(data);
            var child = data.Settable;
            try {
                child.Id = 456;
            } catch(FrozenAccessException) {
                return;
            }
            Assert.Fail("shouldn't have been able to set child property");
        }

        [Test]
        [ExpectedException(typeof(FrozenAccessException))]
        public void Freezing_datagraph_causes_throw_on_setting_of_settable_child() {
            var data = CreateData();
            Freezer.Freeze(data);
            data.Settable = new DataChild() { Id = 123, Name = "foo" };
        }

        private void AssertSameValue(DataGraph first, DataGraph next) {
            Assert.AreEqual(first.Readonly.Id, next.Readonly.Id);
            Assert.AreEqual(first.Readonly.Name, next.Readonly.Name);
            if(first.Settable == null) {
                Assert.IsNull(next.Settable);
                return;
            } 
            Assert.AreEqual(first.Settable.Id, next.Settable.Id);
            Assert.AreEqual(first.Settable.Name, next.Settable.Name);
        }

        private DataGraph CreateData() {
            return Freezer.AsFreezable(new DataGraph { Readonly = { Id = 42, Name = "Everything" } });
        }

        public class DataGraph {
            public DataGraph() {
                Readonly = new DataChild();
            }
            public virtual DataChild Settable { get; set; }
            public virtual DataChild Readonly { get; private set; }
        }

        public class DataChild {
            public virtual int Id { get; set; }
            public virtual string Name { get; set; }
        }
    }
}
