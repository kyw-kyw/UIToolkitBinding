#if UNITY_EDITOR || UNITY_INCLUDE_TESTS

using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIToolkitBinding.Unity.Tests
{
    public class UITKDataSourceObjectTests
    {
        [Test]
        public void SimpleBindTest()
        {
            var user = new UITKUser()
            {
                Name = "Alice"
            };

            user.propertyChanged += (sender, e) =>
            {
                Assert.AreEqual(user, sender);
                Assert.AreEqual(GeneratedEventArgsCache.Name.propertyName, e.propertyName);
            };

            user.Name = "Bob";
        }

        [Test]
        public void WrapperTest()
        {
            (BindablePropertyChangedEventArgs, string) changed = default;
            var user = new User
            {
                Name = "Alice"
            };

            var bindableUser = new BindableUser(user);
            bindableUser.propertyChanged += (sender, e) =>
            {
                Assert.AreEqual(bindableUser, sender);
                changed = (e, bindableUser.Name);
            };

            bindableUser.Name = "Bob";
            Assert.AreEqual(bindableUser.NameChanged.propertyName, changed.Item1.propertyName);
            Assert.AreEqual("Bob", changed.Item2);
            Assert.AreEqual("Bob", bindableUser.UserProxy.Name);
        }

        [Test]
        public void InheritanceTest()
        {
            var data = new Sub();
            data.propertyChanged += (sender, e) =>
            {
                Assert.AreEqual(data, sender);
                Debug.Log($"{e.propertyName} Changed");
            };

            data.BaseData = 1;
            data.SubData = 1;
        }

        [Test]
        public void NestedTypeTest()
        {
            var nest = new Container.Nest();
            nest.propertyChanged += (sender, e) =>
            {
                Assert.AreEqual(GeneratedEventArgsCache.Value.propertyName, e.propertyName);
            };

            nest.Value = 100;
        }
    }
}

#endif
