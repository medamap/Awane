import { ComponentRegistry } from '../../src/registry/ComponentRegistry';
import { AwaneComponent } from '../../src/components/AwaneComponent';
import { IAwaneComponent } from '../../src/components/IAwaneComponent';

interface ITestInterface extends IAwaneComponent {}
interface IAnotherInterface extends IAwaneComponent {}

class TestComponent extends AwaneComponent implements ITestInterface {
    constructor() {
        super();
        (this as any).awaneName = 'TestComponent';
        (this as any).awaneVersion = '1.0.0';
    }
}

class MultiInterfaceComponent extends AwaneComponent implements ITestInterface, IAnotherInterface {
    constructor() {
        super();
        (this as any).awaneName = 'MultiInterfaceComponent';
        (this as any).awaneVersion = '1.0.0';
    }
}

describe('ComponentRegistry', () => {
    let registry: ComponentRegistry;

    beforeEach(() => {
        registry = new ComponentRegistry();
    });

    describe('getComponent', () => {
        it('should return null when registry is empty', () => {
            const result = registry.getComponent('ITestInterface');
            expect(result).toBeNull();
        });

        it('should return component after registration', () => {
            const component = new TestComponent();
            registry.register(component);
            
            const result = registry.getComponent('ITestInterface');
            expect(result).toBe(component);
        });

        it('should retrieve component by each interface when multiple interfaces', () => {
            const component = new MultiInterfaceComponent();
            registry.register(component);
            
            const result1 = registry.getComponent('ITestInterface');
            const result2 = registry.getComponent('IAnotherInterface');
            
            expect(result1).toBe(component);
            expect(result2).toBe(component);
        });

        it('should return first component when multiple components with same interface', () => {
            const component1 = new TestComponent();
            const component2 = new TestComponent();
            
            registry.register(component1);
            registry.register(component2);
            
            const result = registry.getComponent('ITestInterface');
            expect(result).toBe(component1);
        });
    });

    describe('getComponents', () => {
        it('should return empty array when no components', () => {
            const results = registry.getComponents('ITestInterface');
            expect(results).toEqual([]);
        });

        it('should return all components with same interface', () => {
            const component1 = new TestComponent();
            const component2 = new TestComponent();
            
            registry.register(component1);
            registry.register(component2);
            
            const results = registry.getComponents('ITestInterface');
            expect(results).toHaveLength(2);
            expect(results).toContain(component1);
            expect(results).toContain(component2);
        });
    });

    describe('clear', () => {
        it('should remove all components', () => {
            const component = new TestComponent();
            registry.register(component);
            
            registry.clear();
            
            const result = registry.getComponent('ITestInterface');
            expect(result).toBeNull();
        });
    });

    describe('getRegisteredInterfaces', () => {
        it('should return empty array when no interfaces registered', () => {
            const interfaces = registry.getRegisteredInterfaces();
            expect(interfaces).toEqual([]);
        });

        it('should return all registered interface names', () => {
            const component1 = new TestComponent();
            const component2 = new MultiInterfaceComponent();
            
            registry.register(component1);
            registry.register(component2);
            
            const interfaces = registry.getRegisteredInterfaces();
            expect(interfaces).toContain('ITestInterface');
            expect(interfaces).toContain('IAnotherInterface');
            expect(interfaces).toHaveLength(2);
        });
    });
});