import { IAwaneComponent } from '../components/IAwaneComponent';
import { AmbiguousComponentException } from './AmbiguousComponentException';

export class ComponentRegistry {
    private registry: Map<string, IAwaneComponent[]>;

    constructor() {
        this.registry = new Map<string, IAwaneComponent[]>();
    }

    registerComponent(interfaceName: string, component: IAwaneComponent): void {
        if (!interfaceName) {
            throw new Error('Interface name cannot be null or empty');
        }

        if (!component) {
            throw new Error('Component cannot be null or undefined');
        }

        const components = this.registry.get(interfaceName) || [];
        components.push(component);
        this.registry.set(interfaceName, components);
    }

    register(component: IAwaneComponent): void {
        if (!component) {
            throw new Error('Component cannot be null or undefined');
        }

        const interfaces = this.getImplementedInterfaces(component);
        
        interfaces.forEach(interfaceName => {
            const components = this.registry.get(interfaceName) || [];
            components.push(component);
            this.registry.set(interfaceName, components);
        });
    }

    getComponent<T>(interfaceName: string): T | null {
        if (!interfaceName) {
            return null;
        }

        const resolvedName = this.resolveInterfaceName(interfaceName);
        if (!resolvedName) {
            return null;
        }

        const components = this.registry.get(resolvedName);
        return components && components.length > 0 ? components[0] as T : null;
    }

    getComponentBase(interfaceName: string): IAwaneComponent | null {
        if (!interfaceName) {
            return null;
        }

        return this.getComponent<IAwaneComponent>(interfaceName);
    }

    getComponents<T>(interfaceName: string): T[] {
        if (!interfaceName) {
            return [];
        }

        const matches = this.findMatchingInterfaces(interfaceName);
        const result: T[] = [];

        matches.forEach(match => {
            const components = this.registry.get(match);
            if (components) {
                result.push(...components as T[]);
            }
        });

        return result;
    }

    getComponentsBase(interfaceName: string): IAwaneComponent[] {
        return this.getComponents<IAwaneComponent>(interfaceName);
    }

    clear(): void {
        this.registry.clear();
    }

    getRegisteredInterfaces(): string[] {
        return Array.from(this.registry.keys()).sort();
    }

    private getImplementedInterfaces(component: IAwaneComponent): string[] {
        // Use the awaneInterfaces property from the component
        return component.awaneInterfaces || [];
    }

    private resolveInterfaceName(requestedName: string): string | null {
        if (this.registry.has(requestedName)) {
            return requestedName;
        }

        const matches = this.findMatchingInterfaces(requestedName);
        
        if (matches.length === 0) {
            return null;
        }

        if (matches.length === 1) {
            return matches[0];
        }

        throw new AmbiguousComponentException(`Ambiguous interface name '${requestedName}'. Multiple matches found: ${matches.join(', ')}. Use a more specific name.`);
    }

    private findMatchingInterfaces(requestedName: string): string[] {
        const matches: string[] = [];
        
        this.registry.forEach((_, registeredInterface) => {
            if (this.isMatch(registeredInterface, requestedName)) {
                matches.push(registeredInterface);
            }
        });

        return matches;
    }

    private isMatch(fullName: string, suffix: string): boolean {
        if (fullName === suffix) {
            return true;
        }

        if (fullName.endsWith('.' + suffix)) {
            return true;
        }

        // Support partial namespace matching like "Students."
        if (suffix.endsWith('.') && fullName.includes(suffix)) {
            return true;
        }

        return false;
    }
}