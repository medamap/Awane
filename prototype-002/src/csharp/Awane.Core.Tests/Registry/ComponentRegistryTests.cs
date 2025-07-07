using Microsoft.VisualStudio.TestTools.UnitTesting;
using Awane.Core.Components;
using Awane.Core.Registry;
using System.Linq;

namespace Awane.Core.Tests.Registry
{
    [TestClass]
    public class ComponentRegistryTests
    {
        private ComponentRegistry registry = null!;

        [TestInitialize]
        public void Setup()
        {
            registry = new ComponentRegistry();
        }

        [TestMethod]
        public void GetComponent_WhenEmptyRegistry_ReturnsNull()
        {
            var result = registry.GetComponent("ITestInterface");
            
            Assert.IsNull(result);
        }

        [TestMethod]
        public void Register_AndGetComponent_WithSingleComponent_ReturnsComponent()
        {
            var component = new TestComponent();
            
            registry.Register(component);
            var result = registry.GetComponent("Awane.Core.Tests.Registry.ComponentRegistryTests+ITestInterface");
            
            Assert.IsNotNull(result);
            Assert.AreSame(component, result);
        }

        [TestMethod]
        public void Register_WithMultipleInterfaces_CanRetrieveByEachInterface()
        {
            var component = new MultiInterfaceComponent();
            
            registry.Register(component);
            
            var result1 = registry.GetComponent("Awane.Core.Tests.Registry.ComponentRegistryTests+ITestInterface");
            var result2 = registry.GetComponent("Awane.Core.Tests.Registry.ComponentRegistryTests+IAnotherInterface");
            
            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);
            Assert.AreSame(component, result1);
            Assert.AreSame(component, result2);
        }

        [TestMethod]
        public void Register_MultipleComponentsWithSameInterface_GetComponentReturnsFirst()
        {
            var component1 = new TestComponent();
            var component2 = new TestComponent();
            
            registry.Register(component1);
            registry.Register(component2);
            
            var result = registry.GetComponent("Awane.Core.Tests.Registry.ComponentRegistryTests+ITestInterface");
            
            Assert.IsNotNull(result);
            Assert.AreSame(component1, result);
        }

        [TestMethod]
        public void GetComponents_WithMultipleComponents_ReturnsAll()
        {
            var component1 = new TestComponent();
            var component2 = new TestComponent();
            
            registry.Register(component1);
            registry.Register(component2);
            
            var results = registry.GetComponents("Awane.Core.Tests.Registry.ComponentRegistryTests+ITestInterface");
            
            Assert.AreEqual(2, results.Length);
            Assert.IsTrue(results.Contains(component1));
            Assert.IsTrue(results.Contains(component2));
        }

        [TestMethod]
        public void GetComponents_WhenNoComponents_ReturnsEmptyArray()
        {
            var results = registry.GetComponents("ITestInterface");
            
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Length);
        }

        [TestMethod]
        public void Clear_RemovesAllComponents()
        {
            var component = new TestComponent();
            registry.Register(component);
            
            registry.Clear();
            
            var result = registry.GetComponent("Awane.Core.Tests.Registry.ComponentRegistryTests+ITestInterface");
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetRegisteredInterfaces_ReturnsAllInterfaceNames()
        {
            var component1 = new TestComponent();
            var component2 = new MultiInterfaceComponent();
            
            registry.Register(component1);
            registry.Register(component2);
            
            var interfaces = registry.GetRegisteredInterfaces();
            
            Assert.IsTrue(interfaces.Contains("Awane.Core.Tests.Registry.ComponentRegistryTests+ITestInterface"));
            Assert.IsTrue(interfaces.Contains("Awane.Core.Tests.Registry.ComponentRegistryTests+IAnotherInterface"));
            Assert.AreEqual(2, interfaces.Length);
        }

        [TestMethod]
        public void GetRegisteredInterfaces_WhenEmpty_ReturnsEmptyArray()
        {
            var interfaces = registry.GetRegisteredInterfaces();
            
            Assert.IsNotNull(interfaces);
            Assert.AreEqual(0, interfaces.Length);
        }

        private interface ITestInterface { }
        private interface IAnotherInterface { }

        private class TestComponent : AwaneComponent, ITestInterface
        {
            public TestComponent()
            {
                AwaneName = "TestComponent";
                AwaneVersion = "1.0.0";
            }
        }

        private class MultiInterfaceComponent : AwaneComponent, ITestInterface, IAnotherInterface
        {
            public MultiInterfaceComponent()
            {
                AwaneName = "MultiInterfaceComponent";
                AwaneVersion = "1.0.0";
            }
        }
    }
}