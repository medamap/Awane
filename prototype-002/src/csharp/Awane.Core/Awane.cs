using System;
using System.Linq;
using Awane.Core.Components;
using Awane.Core.Registry;

namespace Awane.Core
{
    public static class Awane
    {
        private static readonly ComponentRegistry _registry = new ComponentRegistry();

        public static void Register(IAwaneComponent component)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            _registry.Register(component);
        }

        public static T? GetComponent<T>() where T : class
        {
            var interfaceName = typeof(T).FullName;
            if (interfaceName == null)
            {
                return null;
            }
            var component = _registry.GetComponent(interfaceName);
            return component as T;
        }

        public static IAwaneComponent? GetComponent(string interfaceName)
        {
            return _registry.GetComponent(interfaceName);
        }

        public static T[] GetComponents<T>() where T : class
        {
            var interfaceName = typeof(T).FullName;
            if (interfaceName == null)
            {
                return Array.Empty<T>();
            }
            var components = _registry.GetComponents(interfaceName);
            return components.OfType<T>().ToArray();
        }

        public static IAwaneComponent[] GetComponents(string interfaceName)
        {
            return _registry.GetComponents(interfaceName);
        }

        public static void Reset()
        {
            _registry.Clear();
        }
    }
}