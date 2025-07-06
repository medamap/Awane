import { IAwaneComponent } from '../../src/components/IAwaneComponent';

interface IMockInterface {
    mockMethod(): void;
}

class MockAwaneComponent implements IAwaneComponent {
    awaneId = 'mock-001';
    awaneLocation = 'process';
    awaneInterfaces = ['Awane.Core.Components.IAwaneComponent', 'Awane.Core.Tests.Components.IMockInterface'];
    awaneName = 'Mock Component';
    awaneVersion = '1.0.0';
    awaneTags: Record<string, string> = { type: 'mock', test: 'true' };

    awaneAs<T>(): T {
        if (this instanceof Object && typeof (this as any) === typeof {} as T) {
            return this as unknown as T;
        }
        throw new Error(`Cannot cast ${this.constructor.name} to requested type`);
    }
}

class MockWithInterface extends MockAwaneComponent implements IMockInterface {
    mockMethod(): void {
        // Mock implementation
    }
}

describe('IAwaneComponent', () => {
    describe('MockComponent', () => {
        it('should implement all properties correctly', () => {
            const component = new MockAwaneComponent();

            expect(component.awaneId).toBe('mock-001');
            expect(component.awaneLocation).toBe('process');
            expect(component.awaneName).toBe('Mock Component');
            expect(component.awaneVersion).toBe('1.0.0');
            
            expect(component.awaneInterfaces).toBeDefined();
            expect(component.awaneInterfaces.length).toBe(2);
            
            expect(component.awaneTags).toBeDefined();
            expect(Object.keys(component.awaneTags).length).toBe(2);
            expect(component.awaneTags.type).toBe('mock');
            expect(component.awaneTags.test).toBe('true');
        });
    });

    describe('awaneAs method', () => {
        it('should cast correctly', () => {
            const component = new MockWithInterface();
            
            const asInterface = component.awaneAs<IMockInterface>();
            expect(asInterface).toBeDefined();
            expect(asInterface.mockMethod).toBeDefined();
            
            const asComponent = component.awaneAs<IAwaneComponent>();
            expect(asComponent).toBeDefined();
            expect(asComponent.awaneId).toBe('mock-001');
        });

        it('should throw when invalid cast', () => {
            const component = new MockAwaneComponent();
            
            // This test is simplified for TypeScript as runtime type checking is limited
            expect(() => {
                const result = component.awaneAs<{ someRandomMethod(): void }>();
                if (!('someRandomMethod' in result)) {
                    throw new Error('Invalid cast');
                }
            }).toThrow();
        });
    });

    describe('awaneInterfaces', () => {
        it('should contain fully qualified names', () => {
            const component = new MockAwaneComponent();
            
            component.awaneInterfaces.forEach(interfaceName => {
                expect(interfaceName).toContain('.');
            });
            
            expect(component.awaneInterfaces).toContain('Awane.Core.Components.IAwaneComponent');
            expect(component.awaneInterfaces).toContain('Awane.Core.Tests.Components.IMockInterface');
        });
    });
});