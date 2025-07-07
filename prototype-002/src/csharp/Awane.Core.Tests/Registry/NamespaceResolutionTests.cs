using System;
using System.Linq;
using Awane.Core.Components;
using Awane.Core.Registry;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Awane.Core.Tests.Registry
{
    [TestClass]
    public class NamespaceResolutionTests
    {
        private ComponentRegistry _registry;

        [TestInitialize]
        public void Initialize()
        {
            _registry = new ComponentRegistry();
        }

        [TestMethod]
        public void GetComponent_WithFullyQualifiedName_ReturnsComponent()
        {
            // Arrange
            var component = new TestComponent();
            _registry.RegisterComponent("Tanaka.Cat.IAiri", component);

            // Act
            var result = _registry.GetComponent<IAwaneComponent>("Tanaka.Cat.IAiri");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreSame(component, result);
        }

        [TestMethod]
        public void GetComponent_WithUniqueShortName_ReturnsComponent()
        {
            // Arrange
            var component = new TestComponent();
            _registry.RegisterComponent("Tanaka.Cat.IAiri", component);

            // Act
            var result = _registry.GetComponent<IAwaneComponent>("IAiri");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreSame(component, result);
        }

        [TestMethod]
        public void GetComponent_WithPartialNamespace_ReturnsComponent()
        {
            // Arrange
            var catComponent = new TestComponent();
            var dogComponent = new TestComponent();
            _registry.RegisterComponent("Tanaka.Cat.IAiri", catComponent);
            _registry.RegisterComponent("Tanaka.Dog.IAiri", dogComponent);

            // Act
            var result = _registry.GetComponent<IAwaneComponent>("Dog.IAiri");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreSame(dogComponent, result);
        }

        [TestMethod]
        [ExpectedException(typeof(AmbiguousComponentException))]
        public void GetComponent_WithAmbiguousShortName_ThrowsException()
        {
            // Arrange
            _registry.RegisterComponent("Tanaka.Cat.IAiri", new TestComponent());
            _registry.RegisterComponent("Tanaka.Dog.IAiri", new TestComponent());
            _registry.RegisterComponent("Hirano.Cat.IAiri", new TestComponent());

            // Act & Assert
            _registry.GetComponent<IAwaneComponent>("IAiri");
        }

        [TestMethod]
        public void GetComponent_WithAmbiguousShortName_ExceptionContainsAllMatches()
        {
            // Arrange
            _registry.RegisterComponent("Tanaka.Cat.IAiri", new TestComponent());
            _registry.RegisterComponent("Tanaka.Dog.IAiri", new TestComponent());
            _registry.RegisterComponent("Hirano.Cat.IAiri", new TestComponent());

            // Act & Assert
            try
            {
                _registry.GetComponent<IAwaneComponent>("IAiri");
                Assert.Fail("Expected AmbiguousComponentException");
            }
            catch (AmbiguousComponentException ex)
            {
                Assert.IsTrue(ex.Message.Contains("IAiri"));
                Assert.IsTrue(ex.Message.Contains("Tanaka.Cat.IAiri"));
                Assert.IsTrue(ex.Message.Contains("Tanaka.Dog.IAiri"));
                Assert.IsTrue(ex.Message.Contains("Hirano.Cat.IAiri"));
                Assert.AreEqual(3, ex.MatchedInterfaces.Count());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(AmbiguousComponentException))]
        public void GetComponent_WithAmbiguousPartialNamespace_ThrowsException()
        {
            // Arrange
            _registry.RegisterComponent("Tanaka.Cat.IAiri", new TestComponent());
            _registry.RegisterComponent("Hirano.Cat.IAiri", new TestComponent());

            // Act & Assert
            _registry.GetComponent<IAwaneComponent>("Cat.IAiri");
        }

        [TestMethod]
        public void GetComponent_WithNonExistentName_ReturnsNull()
        {
            // Arrange
            _registry.RegisterComponent("Tanaka.Cat.IAiri", new TestComponent());

            // Act
            var result = _registry.GetComponent<IAwaneComponent>("IPoppo");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetComponent_WithNonExistentPartialName_ReturnsNull()
        {
            // Arrange
            _registry.RegisterComponent("Tanaka.Cat.IAiri", new TestComponent());

            // Act
            var result = _registry.GetComponent<IAwaneComponent>("Dog.IAiri");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetComponent_CaseSensitiveName_ReturnsNull()
        {
            // Arrange
            _registry.RegisterComponent("Tanaka.Cat.IAiri", new TestComponent());

            // Act
            var result1 = _registry.GetComponent<IAwaneComponent>("iairi");
            var result2 = _registry.GetComponent<IAwaneComponent>("cat.IAiri");

            // Assert
            Assert.IsNull(result1);
            Assert.IsNull(result2);
        }

        [TestMethod]
        public void GetComponents_WithPartialNamespace_ReturnsMatchingComponents()
        {
            // Arrange
            var catComponent1 = new TestComponent();
            var catComponent2 = new TestComponent();
            var dogComponent = new TestComponent();
            _registry.RegisterComponent("Tanaka.Cat.IAiri", catComponent1);
            _registry.RegisterComponent("Hirano.Cat.IAiri", catComponent2);
            _registry.RegisterComponent("Tanaka.Dog.IAiri", dogComponent);

            // Act
            var results = _registry.GetComponents<IAwaneComponent>("Cat.IAiri").ToList();

            // Assert
            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.Contains(catComponent1));
            Assert.IsTrue(results.Contains(catComponent2));
            Assert.IsFalse(results.Contains(dogComponent));
        }

        [TestMethod]
        public void GetComponents_WithUniquePartialName_ReturnsSingleComponent()
        {
            // Arrange
            var component = new TestComponent();
            _registry.RegisterComponent("Tanaka.Dog.IAiri", component);
            _registry.RegisterComponent("Tanaka.Cat.IPoppo", new TestComponent());

            // Act
            var results = _registry.GetComponents<IAwaneComponent>("Dog.IAiri").ToList();

            // Assert
            Assert.AreEqual(1, results.Count);
            Assert.AreSame(component, results[0]);
        }

        private class TestComponent : IAwaneComponent
        {
            public string AwaneId => "test-id";
            public string AwaneLocation => "test-location";
            public string[] AwaneInterfaces => new[] { "ITest" };
            public string AwaneName => "TestComponent";
            public string AwaneVersion => "1.0.0";
            public System.Collections.Generic.Dictionary<string, string> AwaneTags => new System.Collections.Generic.Dictionary<string, string>();
            
            public T? AwaneAs<T>() where T : class
            {
                return this as T;
            }
        }
    }
}