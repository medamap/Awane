import { describe, it, expect, beforeEach } from '@jest/globals';
import { register, getComponent, getComponents, reset } from '../src/Awane';
import { AwaneComponent } from '../src/components/AwaneComponent';
import { IAwaneComponent } from '../src/components/IAwaneComponent';

// Test interfaces and components
interface ITestInterface {
    getName(): string;
}

interface IUnregisteredInterface {
    doSomething(): void;
}

class TestComponent extends AwaneComponent implements ITestInterface {
    constructor() {
        super();
        this.addInterface('ITestInterface');
    }

    getName(): string {
        return 'TestComponent';
    }
}

class TestComponent2 extends AwaneComponent implements ITestInterface {
    constructor() {
        super();
        this.addInterface('ITestInterface');
    }

    getName(): string {
        return 'TestComponent2';
    }
}

describe('Awane', () => {
    beforeEach(() => {
        // Reset the registry before each test
        reset();
    });

    it('should register a component successfully', () => {
        // Arrange
        const component = new TestComponent();

        // Act
        register(component);

        // Assert
        const retrieved = getComponent<ITestInterface>('ITestInterface');
        expect(retrieved).toBeDefined();
        expect(retrieved).toBe(component);
    });

    it('should get component with type assertion', () => {
        // Arrange
        const component = new TestComponent();
        register(component);

        // Act
        const retrieved = getComponent<ITestInterface>('ITestInterface');

        // Assert
        expect(retrieved).toBeDefined();
        expect(retrieved?.getName()).toBe('TestComponent');
    });

    it('should get component by string interface name', () => {
        // Arrange
        const component = new TestComponent();
        register(component);

        // Act
        const retrieved = getComponent('ITestInterface');

        // Assert
        expect(retrieved).toBeDefined();
        expect(retrieved).toBe(component);
    });

    it('should get multiple components', () => {
        // Arrange
        const component1 = new TestComponent();
        const component2 = new TestComponent2();
        register(component1);
        register(component2);

        // Act
        const components = getComponents<ITestInterface>('ITestInterface');

        // Assert
        expect(components.length).toBe(2);
        expect(components).toContain(component1);
        expect(components).toContain(component2);
    });

    it('should get multiple components by string', () => {
        // Arrange
        const component1 = new TestComponent();
        const component2 = new TestComponent2();
        register(component1);
        register(component2);

        // Act
        const components = getComponents('ITestInterface');

        // Assert
        expect(components.length).toBe(2);
        expect(components).toContain(component1);
        expect(components).toContain(component2);
    });

    it('should clear all components on reset', () => {
        // Arrange
        const component = new TestComponent();
        register(component);
        expect(getComponent<ITestInterface>('ITestInterface')).toBeDefined();

        // Act
        reset();

        // Assert
        const retrieved = getComponent<ITestInterface>('ITestInterface');
        expect(retrieved).toBeNull();
    });

    it('should throw error when registering null', () => {
        // Act & Assert
        expect(() => register(null as any)).toThrow();
    });

    it('should return null for unregistered interface', () => {
        // Act
        const component = getComponent<IUnregisteredInterface>('IUnregisteredInterface');

        // Assert
        expect(component).toBeNull();
    });

    it('should return null for unregistered interface by string', () => {
        // Act
        const component = getComponent('IUnregisteredInterface');

        // Assert
        expect(component).toBeNull();
    });

    it('should return empty array for unregistered interface', () => {
        // Act
        const components = getComponents<IUnregisteredInterface>('IUnregisteredInterface');

        // Assert
        expect(components).toBeDefined();
        expect(components.length).toBe(0);
    });
});