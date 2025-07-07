using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Awane.Core.Components;

namespace Awane.Core.Tests.Components
{
    public interface ITestInterface
    {
        void TestMethod();
    }

    public interface ITestInterface2
    {
        void TestMethod2();
    }

    public class TestComponent : AwaneComponent, ITestInterface
    {
        public TestComponent() : base()
        {
        }

        public void TestMethod()
        {
        }
    }

    public class TestComponentWithSystemInterface : AwaneComponent, IDisposable, ITestInterface2
    {
        public TestComponentWithSystemInterface() : base()
        {
        }

        public void TestMethod2()
        {
        }

        public void Dispose()
        {
        }
    }

    [TestClass]
    public class AwaneComponentTests
    {
        [TestMethod]
        public void Constructor_ShouldGenerateUniqueAwaneId()
        {
            // Arrange & Act
            var component1 = new TestComponent();
            var component2 = new TestComponent();

            // Assert
            Assert.IsNotNull(component1.AwaneId);
            Assert.IsNotNull(component2.AwaneId);
            Assert.AreNotEqual(component1.AwaneId, component2.AwaneId);
            
            // Verify GUID format
            Assert.IsTrue(Guid.TryParse(component1.AwaneId, out _));
            Assert.IsTrue(Guid.TryParse(component2.AwaneId, out _));
        }

        [TestMethod]
        public void Constructor_ShouldSetDefaultAwaneLocation()
        {
            // Arrange & Act
            var component = new TestComponent();

            // Assert
            Assert.AreEqual("process", component.AwaneLocation);
        }

        [TestMethod]
        public void Constructor_ShouldAutoDetectInterfaces()
        {
            // Arrange & Act
            var component = new TestComponent();

            // Assert
            Assert.IsNotNull(component.AwaneInterfaces);
            Assert.IsTrue(component.AwaneInterfaces.Contains("Awane.Core.Tests.Components.ITestInterface"));
            Assert.IsTrue(component.AwaneInterfaces.Contains("Awane.Core.Components.IAwaneComponent"));
        }

        [TestMethod]
        public void Constructor_ShouldExcludeSystemInterfaces()
        {
            // Arrange & Act
            var component = new TestComponentWithSystemInterface();

            // Assert
            Assert.IsNotNull(component.AwaneInterfaces);
            Assert.IsTrue(component.AwaneInterfaces.Contains("Awane.Core.Tests.Components.ITestInterface2"));
            Assert.IsTrue(component.AwaneInterfaces.Contains("Awane.Core.Components.IAwaneComponent"));
            
            // Should not contain System interfaces
            Assert.IsFalse(component.AwaneInterfaces.Any(i => i.StartsWith("System.")));
            Assert.IsFalse(component.AwaneInterfaces.Contains("System.IDisposable"));
        }

        [TestMethod]
        public void AwaneAs_ShouldReturnCorrectlyCastedInstance()
        {
            // Arrange
            var component = new TestComponent();

            // Act
            var asInterface = component.AwaneAs<ITestInterface>();
            var asComponent = component.AwaneAs<IAwaneComponent>();

            // Assert
            Assert.IsNotNull(asInterface);
            Assert.IsNotNull(asComponent);
            Assert.IsInstanceOfType(asInterface, typeof(ITestInterface));
            Assert.IsInstanceOfType(asComponent, typeof(IAwaneComponent));
            Assert.AreSame(component, asInterface);
            Assert.AreSame(component, asComponent);
        }

        [TestMethod]
        public void AwaneAs_ShouldReturnNullForInvalidCast()
        {
            // Arrange
            var component = new TestComponent();

            // Act
            var asInvalid = component.AwaneAs<ITestInterface2>();

            // Assert
            Assert.IsNull(asInvalid);
        }

        [TestMethod]
        public void DefaultPropertyValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var component = new TestComponent();

            // Assert
            Assert.AreEqual(string.Empty, component.AwaneName);
            Assert.AreEqual("1.0.0", component.AwaneVersion);
            Assert.IsNotNull(component.AwaneTags);
            Assert.AreEqual(0, component.AwaneTags.Count);
        }
    }
}