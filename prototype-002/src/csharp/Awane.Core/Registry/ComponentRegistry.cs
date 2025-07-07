using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Awane.Core.Components;

namespace Awane.Core.Registry
{
    public class ComponentRegistry
    {
        private readonly ConcurrentDictionary<string, List<IAwaneComponent>> registry;
        private readonly object lockObject = new object();

        public ComponentRegistry()
        {
            registry = new ConcurrentDictionary<string, List<IAwaneComponent>>();
        }

        public void Register(IAwaneComponent component)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            var componentType = component.GetType();
            var interfaces = componentType.GetInterfaces()
                .Where(i => i != typeof(IAwaneComponent))
                .ToList();

            foreach (var interfaceType in interfaces)
            {
                var interfaceName = interfaceType.FullName ?? interfaceType.Name;
                
                registry.AddOrUpdate(interfaceName,
                    new List<IAwaneComponent> { component },
                    (key, existingList) =>
                    {
                        lock (lockObject)
                        {
                            var newList = new List<IAwaneComponent>(existingList) { component };
                            return newList;
                        }
                    });
            }
        }

        public IAwaneComponent? GetComponent(string interfaceName)
        {
            if (string.IsNullOrEmpty(interfaceName))
            {
                return null;
            }

            if (registry.TryGetValue(interfaceName, out var components))
            {
                lock (lockObject)
                {
                    return components.FirstOrDefault();
                }
            }

            return null;
        }

        public IAwaneComponent[] GetComponents(string interfaceName)
        {
            if (string.IsNullOrEmpty(interfaceName))
            {
                return Array.Empty<IAwaneComponent>();
            }

            if (registry.TryGetValue(interfaceName, out var components))
            {
                lock (lockObject)
                {
                    return components.ToArray();
                }
            }

            return Array.Empty<IAwaneComponent>();
        }

        public void Clear()
        {
            registry.Clear();
        }

        public string[] GetRegisteredInterfaces()
        {
            return registry.Keys.OrderBy(k => k).ToArray();
        }
    }
}