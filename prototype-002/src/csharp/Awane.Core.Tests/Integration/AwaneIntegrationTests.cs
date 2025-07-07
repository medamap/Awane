using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Awane.Core.Registry;

namespace Awane.Core.Tests.Integration
{
    [TestClass]
    public class AwaneIntegrationTests
    {
        private Airi _airi;
        private Pai _pai;
        private Poppo _poppo;
        private Riko _riko;

        [TestInitialize]
        public void Setup()
        {
            Awane.Reset();
            _airi = new Airi();
            _pai = new Pai();
            _poppo = new Poppo();
            _riko = new Riko();
        }

        [TestMethod]
        public void BasicComponentRegistrationAndRetrieval()
        {
            Awane.Register(_airi);
            Awane.Register(_pai);
            Awane.Register(_poppo);
            Awane.Register(_riko);

            var airiComponent = Awane.GetComponent("PoppoAcademy.Teachers.Airi");
            Assert.IsNotNull(airiComponent);
            
            var airiFromCore = airiComponent.AwaneAs<PoppoAcademy.Core.IPoppoAcademy>();
            Assert.IsNotNull(airiFromCore);
            Assert.AreEqual("Hello from Airi Teacher!", airiFromCore.GetGreeting());

            var airiFromTeacher = airiComponent.AwaneAs<PoppoAcademy.Teachers.IAiri>();
            Assert.IsNotNull(airiFromTeacher);
            Assert.AreEqual("Teaching mathematics with passion!", airiFromTeacher.Teach("mathematics"));

            var paiComponent = Awane.GetComponent("PoppoAcademy.Students.Pai");
            Assert.IsNotNull(paiComponent);
            
            var paiFromStudent = paiComponent.AwaneAs<PoppoAcademy.Students.IPai>();
            Assert.IsNotNull(paiFromStudent);
            Assert.AreEqual("Studying physics intensively!", paiFromStudent.Study("physics"));
        }

        [TestMethod]
        public void GetComponentsForMultipleRetrieval()
        {
            Awane.Register(_airi);
            Awane.Register(_pai);
            Awane.Register(_poppo);
            Awane.Register(_riko);

            var allMembers = Awane.GetComponents<PoppoAcademy.Core.IPoppoAcademy>("PoppoAcademy.Core.IPoppoAcademy");
            Assert.AreEqual(4, allMembers.Length);

            var greetings = allMembers.Select(m => m.GetGreeting()).OrderBy(g => g).ToArray();
            Assert.AreEqual("Hello from Airi Teacher!", greetings[0]);
            Assert.AreEqual("Hello from Pai Student!", greetings[1]);
            Assert.AreEqual("Hello from Poppo Student!", greetings[2]);
            Assert.AreEqual("Hello from Riko Student!", greetings[3]);
        }

        [TestMethod]
        public void NamespaceResolutionIntegrationTest()
        {
            Awane.Register(_airi);
            Awane.Register(_pai);
            Awane.Register(_poppo);
            Awane.Register(_riko);

            var airiByShortName = Awane.GetComponent("IAiri");
            Assert.IsNotNull(airiByShortName);
            Assert.AreEqual("PoppoAcademy.Teachers.Airi", airiByShortName.AwaneId);
            
            var airiAsIAiri = airiByShortName.AwaneAs<PoppoAcademy.Teachers.IAiri>();
            Assert.IsNotNull(airiAsIAiri);

            var paiByShortName = Awane.GetComponent("IPai");
            Assert.IsNotNull(paiByShortName);
            Assert.AreEqual("PoppoAcademy.Students.Pai", paiByShortName.AwaneId);

            var allByShortName = Awane.GetComponents("IPoppoAcademy");
            Assert.AreEqual(4, allByShortName.Length);
        }

        [TestMethod]
        public void AwaneAsMethodTypeConversion()
        {
            Awane.Register(_airi);
            Awane.Register(_pai);

            var component = Awane.GetComponent("PoppoAcademy.Teachers.Airi");
            Assert.IsNotNull(component);

            var asPoppoAcademy = component.AwaneAs<PoppoAcademy.Core.IPoppoAcademy>();
            Assert.IsNotNull(asPoppoAcademy);
            Assert.AreEqual("Hello from Airi Teacher!", asPoppoAcademy.GetGreeting());

            var asAiri = component.AwaneAs<PoppoAcademy.Teachers.IAiri>();
            Assert.IsNotNull(asAiri);
            Assert.AreEqual("Teaching programming with passion!", asAiri.Teach("programming"));

            var asStudent = component.AwaneAs<PoppoAcademy.Students.IPai>();
            Assert.IsNull(asStudent);
        }

        [TestMethod]
        public void ComplexNamespaceResolution()
        {
            Awane.Register(_airi);
            Awane.Register(_pai);
            Awane.Register(_poppo);
            Awane.Register(_riko);

            var airiByPartialNamespace = Awane.GetComponent("Teachers.IAiri");
            Assert.IsNotNull(airiByPartialNamespace);
            Assert.AreEqual("Airi Teacher", airiByPartialNamespace.AwaneName);
            
            var airiAsIAiri = airiByPartialNamespace.AwaneAs<PoppoAcademy.Teachers.IAiri>();
            Assert.IsNotNull(airiAsIAiri);

            var paiByPartialNamespace = Awane.GetComponent("Students.IPai");
            Assert.IsNotNull(paiByPartialNamespace);
            Assert.AreEqual("Pai Student", paiByPartialNamespace.AwaneName);

            var studentsByPartialNamespace = Awane.GetComponents("Students.");
            // We should get 3 students (Pai, Poppo, Riko) but each implements 2 interfaces, 
            // so we expect 6 total results when searching by prefix
            Assert.AreEqual(6, studentsByPartialNamespace.Length);
        }

        [TestMethod]
        public void TagPropertiesVerification()
        {
            Awane.Register(_airi);
            Awane.Register(_pai);
            Awane.Register(_poppo);
            Awane.Register(_riko);

            var teacher = Awane.GetComponent("PoppoAcademy.Teachers.Airi");
            Assert.IsNotNull(teacher);
            Assert.AreEqual("teacher", teacher.AwaneTags["role"]);
            Assert.AreEqual("programming", teacher.AwaneTags["subject"]);

            var advancedStudent = Awane.GetComponent("PoppoAcademy.Students.Pai");
            Assert.IsNotNull(advancedStudent);
            Assert.AreEqual("student", advancedStudent.AwaneTags["role"]);
            Assert.AreEqual("advanced", advancedStudent.AwaneTags["grade"]);

            var beginnerStudent = Awane.GetComponent("PoppoAcademy.Students.Poppo");
            Assert.IsNotNull(beginnerStudent);
            Assert.AreEqual("student", beginnerStudent.AwaneTags["role"]);
            Assert.AreEqual("beginner", beginnerStudent.AwaneTags["grade"]);

            var intermediateStudent = Awane.GetComponent("PoppoAcademy.Students.Riko");
            Assert.IsNotNull(intermediateStudent);
            Assert.AreEqual("student", intermediateStudent.AwaneTags["role"]);
            Assert.AreEqual("intermediate", intermediateStudent.AwaneTags["grade"]);
        }

        [TestMethod]
        public void PerformanceTest()
        {
            var stopwatch = new Stopwatch();
            
            stopwatch.Start();
            for (int i = 0; i < 1000; i++)
            {
                var component = new TestComponent($"Component{i}", $"Test.Component{i}");
                Awane.Register(component);
            }
            stopwatch.Stop();
            
            var registrationTime = stopwatch.ElapsedMilliseconds;
            Assert.IsTrue(registrationTime < 1000, $"Registration of 1000 components took {registrationTime}ms");

            stopwatch.Restart();
            for (int i = 0; i < 1000; i++)
            {
                var component = Awane.GetComponent($"Component{i}");
                Assert.IsNotNull(component);
            }
            stopwatch.Stop();
            
            var retrievalTime = stopwatch.ElapsedMilliseconds;
            Assert.IsTrue(retrievalTime < 1000, $"Retrieval of 1000 components took {retrievalTime}ms");

            stopwatch.Restart();
            var allComponents = Awane.GetComponents<PoppoAcademy.Core.IPoppoAcademy>("PoppoAcademy.Core.IPoppoAcademy");
            stopwatch.Stop();
            
            var getAllTime = stopwatch.ElapsedMilliseconds;
            Assert.IsTrue(getAllTime < 100, $"GetComponents took {getAllTime}ms");
        }

        [TestMethod]
        public void ErrorHandlingAndEdgeCases()
        {
            Assert.ThrowsException<ArgumentNullException>(() => Awane.Register(null!));

            var nonExistent = Awane.GetComponent("NonExistent.Component");
            Assert.IsNull(nonExistent);

            var emptyList = Awane.GetComponents<PoppoAcademy.Core.IPoppoAcademy>("PoppoAcademy.Core.IPoppoAcademy");
            Assert.AreEqual(0, emptyList.Length);

            Awane.Register(_airi);
            Assert.ThrowsException<InvalidOperationException>(() => Awane.Register(_airi));

            // Test ambiguous component resolution
            // Create two components with interfaces that end with 'ITest'
            var testComp1 = new TestComponent("TestComp1", "Test.A.ITest");
            var testComp2 = new TestComponent("TestComp2", "Test.B.ITest");
            Awane.Register(testComp1);
            Awane.Register(testComp2);
            Assert.ThrowsException<AmbiguousComponentException>(() => Awane.GetComponent("ITest"));
        }

        private class TestComponent : PoppoAcademy.Core.IPoppoAcademy
        {
            private readonly string _id;
            private readonly string _interfaceName;

            public TestComponent(string id, string interfaceName)
            {
                _id = id;
                _interfaceName = interfaceName;
            }

            public string AwaneId => _id;
            public string AwaneLocation => "local";
            public string[] AwaneInterfaces => new[] { _interfaceName, "PoppoAcademy.Core.IPoppoAcademy" };
            public string AwaneName => $"Test {_id}";
            public string AwaneVersion => "1.0.0";
            public Dictionary<string, string> AwaneTags => new Dictionary<string, string> { { "test", "true" } };

            public T? AwaneAs<T>() where T : class => this as T;
            public string GetGreeting() => $"Hello from {_id}!";
        }
    }
}