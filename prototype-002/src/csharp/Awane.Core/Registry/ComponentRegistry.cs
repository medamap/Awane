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

        public void RegisterComponent(string interfaceName, IAwaneComponent component)
        {
            if (string.IsNullOrEmpty(interfaceName))
            {
                throw new ArgumentNullException(nameof(interfaceName));
            }

            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

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

        public T? GetComponent<T>(string interfaceName) where T : class
        {
            if (string.IsNullOrEmpty(interfaceName))
            {
                return null;
            }

            var resolvedName = ResolveInterfaceName(interfaceName);
            if (resolvedName == null)
            {
                return null;
            }

            if (registry.TryGetValue(resolvedName, out var components))
            {
                lock (lockObject)
                {
                    return components.FirstOrDefault() as T;
                }
            }

            return null;
        }

        public IAwaneComponent? GetComponent(string interfaceName)
        {
            if (string.IsNullOrEmpty(interfaceName))
            {
                return null;
            }

            return GetComponent<IAwaneComponent>(interfaceName);
        }

        public IEnumerable<T> GetComponents<T>(string interfaceName) where T : class
        {
            if (string.IsNullOrEmpty(interfaceName))
            {
                return Enumerable.Empty<T>();
            }

            var matches = FindMatchingInterfaces(interfaceName);
            var result = new List<T>();

            foreach (var match in matches)
            {
                if (registry.TryGetValue(match, out var components))
                {
                    lock (lockObject)
                    {
                        result.AddRange(components.OfType<T>());
                    }
                }
            }

            return result;
        }

        public IAwaneComponent[] GetComponents(string interfaceName)
        {
            return GetComponents<IAwaneComponent>(interfaceName).ToArray();
        }

        public void Clear()
        {
            registry.Clear();
        }

        public string[] GetRegisteredInterfaces()
        {
            return registry.Keys.OrderBy(k => k).ToArray();
        }

        private string? ResolveInterfaceName(string requestedName)
        {
            if (registry.ContainsKey(requestedName))
            {
                return requestedName;
            }

            var matches = FindMatchingInterfaces(requestedName);
            
            if (matches.Count == 0)
            {
                return null;
            }

            if (matches.Count == 1)
            {
                return matches[0];
            }

            throw new AmbiguousComponentException(requestedName, matches);
        }

        private List<string> FindMatchingInterfaces(string requestedName)
        {
            var matches = new List<string>();
            
            foreach (var registeredInterface in registry.Keys)
            {
                if (IsMatch(registeredInterface, requestedName))
                {
                    matches.Add(registeredInterface);
                }
            }

            return matches;
        }

        private bool IsMatch(string fullName, string suffix)
        {
            if (fullName == suffix)
            {
                return true;
            }

            if (fullName.EndsWith("." + suffix))
            {
                return true;
            }

            // Support partial namespace matching like "Students."
            if (suffix.EndsWith(".") && fullName.Contains(suffix))
            {
                return true;
            }

            return false;
        }
    }
}