import { IAwaneComponent } from './components/IAwaneComponent';
import { ComponentRegistry } from './registry/ComponentRegistry';

// Internal registry instance
const registry = new ComponentRegistry();

/**
 * Register a component in the Awane registry
 * @param component The component to register
 * @throws Error if component is null or undefined
 */
export function register(component: IAwaneComponent): void {
    if (!component) {
        throw new Error('Component cannot be null or undefined');
    }
    registry.register(component);
}

/**
 * Get a component by interface name with optional type assertion
 * @param interfaceName The interface name to search for
 * @returns The component if found, null otherwise
 */
export function getComponent<T = IAwaneComponent>(interfaceName: string): T | null {
    const component = registry.getComponent(interfaceName);
    return component as T | null;
}

/**
 * Get all components implementing a specific interface with optional type assertion
 * @param interfaceName The interface name to search for
 * @returns Array of components implementing the interface
 */
export function getComponents<T = IAwaneComponent>(interfaceName: string): T[] {
    const components = registry.getComponents(interfaceName);
    return components as T[];
}

/**
 * Reset the registry, clearing all registered components
 * Mainly used for testing to ensure clean state between tests
 */
export function reset(): void {
    registry.clear();
}

// Re-export common types for convenience
export { IAwaneComponent } from './components/IAwaneComponent';
export { AwaneComponent } from './components/AwaneComponent';