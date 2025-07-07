import { IAwaneComponent } from '../components/IAwaneComponent';

export class ComponentRegistry {
    private registry: Map<string, IAwaneComponent[]>;

    constructor() {
        this.registry = new Map<string, IAwaneComponent[]>();
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

    getComponent(interfaceName: string): IAwaneComponent | null {
        if (!interfaceName) {
            return null;
        }

        const components = this.registry.get(interfaceName);
        return components && components.length > 0 ? components[0] : null;
    }

    getComponents(interfaceName: string): IAwaneComponent[] {
        if (!interfaceName) {
            return [];
        }

        const components = this.registry.get(interfaceName);
        return components ? [...components] : [];
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
}