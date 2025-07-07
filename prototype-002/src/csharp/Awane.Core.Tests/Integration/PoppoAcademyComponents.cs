using System;
using System.Collections.Generic;
using Awane.Core.Components;

namespace Awane.Core.Tests.Integration
{
    namespace PoppoAcademy.Core
    {
        public interface IPoppoAcademy : IAwaneComponent
        {
            string GetGreeting();
        }
    }

    namespace PoppoAcademy.Teachers
    {
        public interface IAiri : IAwaneComponent
        {
            string Teach(string topic);
        }
    }

    namespace PoppoAcademy.Students
    {
        public interface IPai : IAwaneComponent
        {
            string Study(string subject);
        }

        public interface IPoppo : IAwaneComponent
        {
            string Learn(string lesson);
        }

        public interface IRiko : IAwaneComponent
        {
            string Practice(string skill);
        }
    }

    public class Airi : PoppoAcademy.Core.IPoppoAcademy, PoppoAcademy.Teachers.IAiri
    {
        public string AwaneId => "PoppoAcademy.Teachers.Airi";
        public string AwaneLocation => "local";
        public string[] AwaneInterfaces => new[] { 
            "PoppoAcademy.Core.IPoppoAcademy", 
            "PoppoAcademy.Teachers.IAiri" 
        };
        public string AwaneName => "Airi Teacher";
        public string AwaneVersion => "2.0.0";
        public Dictionary<string, string> AwaneTags => new Dictionary<string, string>
        {
            { "role", "teacher" },
            { "subject", "programming" }
        };

        public T? AwaneAs<T>() where T : class
        {
            return this as T;
        }

        public string GetGreeting()
        {
            return "Hello from Airi Teacher!";
        }

        public string Teach(string topic)
        {
            return $"Teaching {topic} with passion!";
        }
    }

    public class Pai : PoppoAcademy.Core.IPoppoAcademy, PoppoAcademy.Students.IPai
    {
        public string AwaneId => "PoppoAcademy.Students.Pai";
        public string AwaneLocation => "local";
        public string[] AwaneInterfaces => new[] { 
            "PoppoAcademy.Core.IPoppoAcademy", 
            "PoppoAcademy.Students.IPai" 
        };
        public string AwaneName => "Pai Student";
        public string AwaneVersion => "1.0.0";
        public Dictionary<string, string> AwaneTags => new Dictionary<string, string>
        {
            { "role", "student" },
            { "grade", "advanced" }
        };

        public T? AwaneAs<T>() where T : class
        {
            return this as T;
        }

        public string GetGreeting()
        {
            return "Hello from Pai Student!";
        }

        public string Study(string subject)
        {
            return $"Studying {subject} intensively!";
        }
    }

    public class Poppo : PoppoAcademy.Core.IPoppoAcademy, PoppoAcademy.Students.IPoppo
    {
        public string AwaneId => "PoppoAcademy.Students.Poppo";
        public string AwaneLocation => "local";
        public string[] AwaneInterfaces => new[] { 
            "PoppoAcademy.Core.IPoppoAcademy", 
            "PoppoAcademy.Students.IPoppo" 
        };
        public string AwaneName => "Poppo Student";
        public string AwaneVersion => "1.0.0";
        public Dictionary<string, string> AwaneTags => new Dictionary<string, string>
        {
            { "role", "student" },
            { "grade", "beginner" }
        };

        public T? AwaneAs<T>() where T : class
        {
            return this as T;
        }

        public string GetGreeting()
        {
            return "Hello from Poppo Student!";
        }

        public string Learn(string lesson)
        {
            return $"Learning {lesson} step by step!";
        }
    }

    public class Riko : PoppoAcademy.Core.IPoppoAcademy, PoppoAcademy.Students.IRiko
    {
        public string AwaneId => "PoppoAcademy.Students.Riko";
        public string AwaneLocation => "local";
        public string[] AwaneInterfaces => new[] { 
            "PoppoAcademy.Core.IPoppoAcademy", 
            "PoppoAcademy.Students.IRiko" 
        };
        public string AwaneName => "Riko Student";
        public string AwaneVersion => "1.5.0";
        public Dictionary<string, string> AwaneTags => new Dictionary<string, string>
        {
            { "role", "student" },
            { "grade", "intermediate" }
        };

        public T? AwaneAs<T>() where T : class
        {
            return this as T;
        }

        public string GetGreeting()
        {
            return "Hello from Riko Student!";
        }

        public string Practice(string skill)
        {
            return $"Practicing {skill} diligently!";
        }
    }
}