import { ComponentRegistry } from '../../src/registry/ComponentRegistry';
import { IAwaneComponent } from '../../src/components/IAwaneComponent';

class TestComponent implements IAwaneComponent {
    public readonly awaneId = 'test-id';
    public readonly awaneLocation = 'test-location';
    public readonly awaneInterfaces = ['ITest'];
    public readonly awaneName = 'TestComponent';
    public readonly awaneVersion = '1.0.0';
    public readonly awaneTags = {};

    awaneAs<T>(): T {
        return (this as unknown) as T;
    }
}

describe('NamespaceResolutionTests', () => {
    let registry: ComponentRegistry;

    beforeEach(() => {
        registry = new ComponentRegistry();
    });

    test('getComponent with fully qualified name returns component', () => {
        // Arrange
        const component = new TestComponent();
        registry.registerComponent('Tanaka.Cat.IAiri', component);

        // Act
        const result = registry.getComponent<IAwaneComponent>('Tanaka.Cat.IAiri');

        // Assert
        expect(result).toBe(component);
    });

    test('getComponent with unique short name returns component', () => {
        // Arrange
        const component = new TestComponent();
        registry.registerComponent('Tanaka.Cat.IAiri', component);

        // Act
        const result = registry.getComponent<IAwaneComponent>('IAiri');

        // Assert
        expect(result).toBe(component);
    });

    test('getComponent with partial namespace returns component', () => {
        // Arrange
        const catComponent = new TestComponent();
        const dogComponent = new TestComponent();
        registry.registerComponent('Tanaka.Cat.IAiri', catComponent);
        registry.registerComponent('Tanaka.Dog.IAiri', dogComponent);

        // Act
        const result = registry.getComponent<IAwaneComponent>('Dog.IAiri');

        // Assert
        expect(result).toBe(dogComponent);
    });

    test('getComponent with ambiguous short name throws exception', () => {
        // Arrange
        registry.registerComponent('Tanaka.Cat.IAiri', new TestComponent());
        registry.registerComponent('Tanaka.Dog.IAiri', new TestComponent());
        registry.registerComponent('Hirano.Cat.IAiri', new TestComponent());

        // Act & Assert
        expect(() => {
            registry.getComponent<IAwaneComponent>('IAiri');
        }).toThrow('Ambiguous interface name');
    });

    test('getComponent with ambiguous short name exception contains all matches', () => {
        // Arrange
        registry.registerComponent('Tanaka.Cat.IAiri', new TestComponent());
        registry.registerComponent('Tanaka.Dog.IAiri', new TestComponent());
        registry.registerComponent('Hirano.Cat.IAiri', new TestComponent());

        // Act & Assert
        try {
            registry.getComponent<IAwaneComponent>('IAiri');
            fail('Expected error to be thrown');
        } catch (error: any) {
            expect(error.message).toContain('IAiri');
            expect(error.message).toContain('Tanaka.Cat.IAiri');
            expect(error.message).toContain('Tanaka.Dog.IAiri');
            expect(error.message).toContain('Hirano.Cat.IAiri');
        }
    });

    test('getComponent with ambiguous partial namespace throws exception', () => {
        // Arrange
        registry.registerComponent('Tanaka.Cat.IAiri', new TestComponent());
        registry.registerComponent('Hirano.Cat.IAiri', new TestComponent());

        // Act & Assert
        expect(() => {
            registry.getComponent<IAwaneComponent>('Cat.IAiri');
        }).toThrow('Ambiguous interface name');
    });

    test('getComponent with non-existent name returns null', () => {
        // Arrange
        registry.registerComponent('Tanaka.Cat.IAiri', new TestComponent());

        // Act
        const result = registry.getComponent<IAwaneComponent>('IPoppo');

        // Assert
        expect(result).toBeNull();
    });

    test('getComponent with non-existent partial name returns null', () => {
        // Arrange
        registry.registerComponent('Tanaka.Cat.IAiri', new TestComponent());

        // Act
        const result = registry.getComponent<IAwaneComponent>('Dog.IAiri');

        // Assert
        expect(result).toBeNull();
    });

    test('getComponent case sensitive name returns null', () => {
        // Arrange
        registry.registerComponent('Tanaka.Cat.IAiri', new TestComponent());

        // Act
        const result1 = registry.getComponent<IAwaneComponent>('iairi');
        const result2 = registry.getComponent<IAwaneComponent>('cat.IAiri');

        // Assert
        expect(result1).toBeNull();
        expect(result2).toBeNull();
    });

    test('getComponents with partial namespace returns matching components', () => {
        // Arrange
        const catComponent1 = new TestComponent();
        const catComponent2 = new TestComponent();
        const dogComponent = new TestComponent();
        registry.registerComponent('Tanaka.Cat.IAiri', catComponent1);
        registry.registerComponent('Hirano.Cat.IAiri', catComponent2);
        registry.registerComponent('Tanaka.Dog.IAiri', dogComponent);

        // Act
        const results = registry.getComponents<IAwaneComponent>('Cat.IAiri');

        // Assert
        expect(results).toHaveLength(2);
        expect(results).toContain(catComponent1);
        expect(results).toContain(catComponent2);
        expect(results).not.toContain(dogComponent);
    });

    test('getComponents with unique partial name returns single component', () => {
        // Arrange
        const component = new TestComponent();
        registry.registerComponent('Tanaka.Dog.IAiri', component);
        registry.registerComponent('Tanaka.Cat.IPoppo', new TestComponent());

        // Act
        const results = registry.getComponents<IAwaneComponent>('Dog.IAiri');

        // Assert
        expect(results).toHaveLength(1);
        expect(results[0]).toBe(component);
    });
});