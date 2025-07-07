using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Awane.Core.Components
{
    public abstract class AwaneComponent : IAwaneComponent
    {
        private readonly string _awaneId;
        private readonly string[] _awaneInterfaces;
        private readonly Dictionary<string, string> _awaneTags;

        public string AwaneId => _awaneId;
        
        public string AwaneLocation { get; protected set; }
        
        public string[] AwaneInterfaces => _awaneInterfaces;
        
        public string AwaneName { get; protected set; }
        
        public string AwaneVersion { get; protected set; }
        
        public Dictionary<string, string> AwaneTags => _awaneTags;

        protected AwaneComponent()
        {
            _awaneId = Guid.NewGuid().ToString();
            AwaneLocation = "process";
            AwaneName = string.Empty;
            AwaneVersion = "1.0.0";
            _awaneTags = new Dictionary<string, string>();
            
            // Get all interfaces implemented by this class
            var interfaces = GetType()
                .GetInterfaces()
                .Where(i => !IsSystemInterface(i))
                .Select(i => i.FullName)
                .Where(name => name != null)
                .Cast<string>()
                .ToArray();
            
            _awaneInterfaces = interfaces;
        }

        public T? AwaneAs<T>() where T : class
        {
            return this as T;
        }

        private bool IsSystemInterface(Type interfaceType)
        {
            return interfaceType.Namespace != null && 
                   interfaceType.Namespace.StartsWith("System");
        }
    }
}