import { AwaneComponent } from '../../src/components/AwaneComponent';
import { IAwaneComponent } from '../../src/components/IAwaneComponent';

interface ITestInterface {
    testMethod(): void;
}

interface ITestInterface2 {
    testMethod2(): void;
}

class TestComponent extends AwaneComponent implements ITestInterface {
    constructor() {
        super();
        this.addInterface('ITestInterface');
    }

    testMethod(): void {
        // Test implementation
    }
}

class TestComponentWithMultipleInterfaces extends AwaneComponent implements ITestInterface, ITestInterface2 {
    constructor() {
        super();
        this.addInterface('ITestInterface');
        this.addInterface('ITestInterface2');
    }

    testMethod(): void {
        // Test implementation
    }

    testMethod2(): void {
        // Test implementation
    }
}

describe('AwaneComponent', () => {
    describe('constructor', () => {
        it('should generate unique awaneId for each instance', () => {
            // Arrange & Act
            const component1 = new TestComponent();
            const component2 = new TestComponent();

            // Assert
            expect(component1.awaneId).toBeDefined();
            expect(component2.awaneId).toBeDefined();
            expect(component1.awaneId).not.toBe(component2.awaneId);
            
            // Verify UUID format (simple check)
            expect(component1.awaneId).toMatch(/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i);
            expect(component2.awaneId).toMatch(/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i);
        });

        it('should set default awaneLocation to "process"', () => {
            // Arrange & Act
            const component = new TestComponent();

            // Assert
            expect(component.awaneLocation).toBe('process');
        });

        it('should initialize empty awaneInterfaces array', () => {
            // Arrange & Act
            class EmptyComponent extends AwaneComponent {
                constructor() {
                    super();
                }
            }
            const component = new EmptyComponent();

            // Assert
            expect(component.awaneInterfaces).toBeDefined();
            expect(Array.isArray(component.awaneInterfaces)).toBe(true);
            expect(component.awaneInterfaces).toHaveLength(0);
        });

        it('should set default property values', () => {
            // Arrange & Act
            const component = new TestComponent();

            // Assert
            expect(component.awaneName).toBe('');
            expect(component.awaneVersion).toBe('1.0.0');
            expect(component.awaneTags).toBeDefined();
            expect(Object.keys(component.awaneTags)).toHaveLength(0);
        });
    });

    describe('addInterface', () => {
        it('should add interface name to awaneInterfaces array', () => {
            // Arrange
            const component = new TestComponent();

            // Act & Assert
            expect(component.awaneInterfaces).toContain('ITestInterface');
        });

        it('should handle multiple interfaces', () => {
            // Arrange
            const component = new TestComponentWithMultipleInterfaces();

            // Assert
            expect(component.awaneInterfaces).toContain('ITestInterface');
            expect(component.awaneInterfaces).toContain('ITestInterface2');
            expect(component.awaneInterfaces).toHaveLength(2);
        });

        it('should not add duplicate interfaces', () => {
            // Arrange
            class TestDuplicateComponent extends AwaneComponent {
                constructor() {
                    super();
                }
            }
            const component = new TestDuplicateComponent();

            // Act
            component.addInterface('ITestInterface');
            component.addInterface('ITestInterface');

            // Assert
            expect(component.awaneInterfaces).toHaveLength(1);
            expect(component.awaneInterfaces).toContain('ITestInterface');
        });
    });

    describe('awaneAs', () => {
        it('should return correctly casted instance', () => {
            // Arrange
            const component = new TestComponent();

            // Act
            const asInterface = component.awaneAs<ITestInterface>();
            const asComponent = component.awaneAs<IAwaneComponent>();

            // Assert
            expect(asInterface).toBeDefined();
            expect(asComponent).toBeDefined();
            expect(asInterface).toBe(component);
            expect(asComponent).toBe(component);
            expect(asInterface.testMethod).toBeDefined();
        });

        it('should return instance even when casting to unimplemented interface', () => {
            // Arrange
            const component = new TestComponent();

            // Act
            const asInvalid = component.awaneAs<ITestInterface2>();

            // Assert
            // In TypeScript, the cast will succeed at compile time
            // Runtime type checking would be needed for actual validation
            expect(asInvalid).toBe(component);
        });
    });
});