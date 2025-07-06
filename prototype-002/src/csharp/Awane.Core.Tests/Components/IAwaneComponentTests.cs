using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Awane.Core.Components;

namespace Awane.Core.Tests.Components
{
    [TestClass]
    public class IAwaneComponentTests
    {
        private class MockAwaneComponent : IAwaneComponent
        {
            public string AwaneId { get; set; } = "mock-001";
            public string AwaneLocation { get; set; } = "process";
            public string[] AwaneInterfaces { get; set; } = ["Awane.Core.Components.IAwaneComponent", "Awane.Core.Tests.Components.IMockInterface"];
            public string AwaneName { get; set; } = "Mock Component";
            public string AwaneVersion { get; set; } = "1.0.0";
            public Dictionary<string, string> AwaneTags { get; set; } = new() { ["type"] = "mock", ["test"] = "true" };

            public T AwaneAs<T>() where T : class
            {
                if (this is T result)
                {
                    return result;
                }
                throw new InvalidCastException($"Cannot cast {GetType().FullName} to {typeof(T).FullName}");
            }
        }

        private interface IMockInterface
        {
            void MockMethod();
        }

        private class MockWithInterface : MockAwaneComponent, IMockInterface
        {
            public void MockMethod() { }
        }

        [TestMethod]
        public void MockComponent_ShouldImplementAllPropertiesCorrectly()
        {
            var component = new MockAwaneComponent();

            Assert.AreEqual("mock-001", component.AwaneId);
            Assert.AreEqual("process", component.AwaneLocation);
            Assert.AreEqual("Mock Component", component.AwaneName);
            Assert.AreEqual("1.0.0", component.AwaneVersion);
            
            Assert.IsNotNull(component.AwaneInterfaces);
            Assert.AreEqual(2, component.AwaneInterfaces.Length);
            
            Assert.IsNotNull(component.AwaneTags);
            Assert.AreEqual(2, component.AwaneTags.Count);
            Assert.AreEqual("mock", component.AwaneTags["type"]);
            Assert.AreEqual("true", component.AwaneTags["test"]);
        }

        [TestMethod]
        public void AwaneAs_ShouldCastCorrectly()
        {
            var component = new MockWithInterface();
            
            var asInterface = component.AwaneAs<IMockInterface>();
            Assert.IsNotNull(asInterface);
            Assert.IsInstanceOfType(asInterface, typeof(IMockInterface));
            
            var asComponent = component.AwaneAs<IAwaneComponent>();
            Assert.IsNotNull(asComponent);
            Assert.IsInstanceOfType(asComponent, typeof(IAwaneComponent));
        }

        [TestMethod]
        public void AwaneAs_ShouldThrowWhenInvalidCast()
        {
            var component = new MockAwaneComponent();
            
            Assert.ThrowsException<InvalidCastException>(() =>
            {
                component.AwaneAs<IMockInterface>();
            });
        }

        [TestMethod]
        public void AwaneInterfaces_ShouldContainFullyQualifiedNames()
        {
            var component = new MockAwaneComponent();
            
            foreach (var interfaceName in component.AwaneInterfaces)
            {
                Assert.IsTrue(interfaceName.Contains("."), $"Interface name '{interfaceName}' should be fully qualified");
            }
            
            Assert.IsTrue(component.AwaneInterfaces.Contains("Awane.Core.Components.IAwaneComponent"));
            Assert.IsTrue(component.AwaneInterfaces.Contains("Awane.Core.Tests.Components.IMockInterface"));
        }
    }
}