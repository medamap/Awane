using Microsoft.VisualStudio.TestTools.UnitTesting;
using Awane.Core;
using Awane.Core.Components;
using System;
using System.Linq;

namespace Awane.Core.Tests
{
    [TestClass]
    public class AwaneTests
    {
        [TestInitialize]
        public void Setup()
        {
            // 各テストの前にレジストリをリセット
            Awane.Reset();
        }

        [TestMethod]
        public void Register_WithValidComponent_RegistersSuccessfully()
        {
            // Arrange
            var component = new TestComponent();

            // Act
            Awane.Register(component);

            // Assert
            var retrieved = Awane.GetComponent("Awane.Core.Tests.AwaneTests+ITestInterface");
            Assert.IsNotNull(retrieved);
            Assert.AreEqual(component, retrieved);
        }

        [TestMethod]
        public void GetComponent_Generic_ReturnsTypedComponent()
        {
            // Arrange
            var component = new TestComponent();
            Awane.Register(component);

            // Act
            var retrieved = Awane.GetComponent<ITestInterface>();

            // Assert
            Assert.IsNotNull(retrieved);
            Assert.IsInstanceOfType(retrieved, typeof(ITestInterface));
            Assert.AreEqual(component, retrieved);
        }

        [TestMethod]
        public void GetComponent_ByString_ReturnsComponent()
        {
            // Arrange
            var component = new TestComponent();
            Awane.Register(component);

            // Act
            var retrieved = Awane.GetComponent("Awane.Core.Tests.AwaneTests+ITestInterface");

            // Assert
            Assert.IsNotNull(retrieved);
            Assert.AreEqual(component, retrieved);
        }

        [TestMethod]
        public void GetComponents_Generic_ReturnsMultipleComponents()
        {
            // Arrange
            var component1 = new TestComponent();
            var component2 = new TestComponent2();
            Awane.Register(component1);
            Awane.Register(component2);

            // Act
            var components = Awane.GetComponents<ITestInterface>();

            // Assert
            Assert.AreEqual(2, components.Length);
            Assert.IsTrue(components.Contains(component1));
            Assert.IsTrue(components.Contains(component2));
        }

        [TestMethod]
        public void GetComponents_ByString_ReturnsMultipleComponents()
        {
            // Arrange
            var component1 = new TestComponent();
            var component2 = new TestComponent2();
            Awane.Register(component1);
            Awane.Register(component2);

            // Act
            var components = Awane.GetComponents("Awane.Core.Tests.AwaneTests+ITestInterface");

            // Assert
            Assert.AreEqual(2, components.Length);
            Assert.IsTrue(components.Contains(component1));
            Assert.IsTrue(components.Contains(component2));
        }

        [TestMethod]
        public void Reset_ClearsAllComponents()
        {
            // Arrange
            var component = new TestComponent();
            Awane.Register(component);
            Assert.IsNotNull(Awane.GetComponent<ITestInterface>());

            // Act
            Awane.Reset();

            // Assert
            var retrieved = Awane.GetComponent<ITestInterface>();
            Assert.IsNull(retrieved);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Register_WithNull_ThrowsException()
        {
            // Act
            Awane.Register(null!);
        }

        [TestMethod]
        public void GetComponent_ForUnregisteredInterface_ReturnsNull()
        {
            // Act
            var component = Awane.GetComponent<IUnregisteredInterface>();

            // Assert
            Assert.IsNull(component);
        }

        [TestMethod]
        public void GetComponent_ByString_ForUnregisteredInterface_ReturnsNull()
        {
            // Act
            var component = Awane.GetComponent("IUnregisteredInterface");

            // Assert
            Assert.IsNull(component);
        }

        [TestMethod]
        public void GetComponents_ForUnregisteredInterface_ReturnsEmptyArray()
        {
            // Act
            var components = Awane.GetComponents<IUnregisteredInterface>();

            // Assert
            Assert.IsNotNull(components);
            Assert.AreEqual(0, components.Length);
        }

        // Test interfaces and components
        private interface ITestInterface
        {
            string GetName();
        }

        private interface IUnregisteredInterface
        {
            void DoSomething();
        }

        private class TestComponent : AwaneComponent, ITestInterface
        {
            public string GetName() => "TestComponent";
        }

        private class TestComponent2 : AwaneComponent, ITestInterface
        {
            public string GetName() => "TestComponent2";
        }
    }
}