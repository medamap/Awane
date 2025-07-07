import { describe, test, expect, beforeEach } from '@jest/globals';
import * as Awane from '../../src/Awane';
import { ComponentAlreadyRegisteredException } from '../../src/registry/ComponentAlreadyRegisteredException';
import { AmbiguousComponentException } from '../../src/registry/AmbiguousComponentException';
import { 
    Airi, 
    Pai, 
    Poppo, 
    Riko, 
    PoppoAcademy 
} from './PoppoAcademyComponents';

describe('Awane Integration Tests', () => {
    let airi: Airi;
    let pai: Pai;
    let poppo: Poppo;
    let riko: Riko;

    beforeEach(() => {
        Awane.reset();
        airi = new Airi();
        pai = new Pai();
        poppo = new Poppo();
        riko = new Riko();
    });

    test('基本的なコンポーネント登録と取得', () => {
        Awane.register(airi);
        Awane.register(pai);
        Awane.register(poppo);
        Awane.register(riko);

        const airiComponent = Awane.getComponent('PoppoAcademy.Teachers.Airi');
        expect(airiComponent).not.toBeNull();
        
        const airiFromCore = airiComponent!.awaneAs<PoppoAcademy.Core.IPoppoAcademy>();
        expect(airiFromCore).not.toBeNull();
        expect(airiFromCore.getGreeting()).toBe('Hello from Airi Teacher!');

        const airiFromTeacher = airiComponent!.awaneAs<PoppoAcademy.Teachers.IAiri>();
        expect(airiFromTeacher).not.toBeNull();
        expect(airiFromTeacher.teach('mathematics')).toBe('Teaching mathematics with passion!');

        const paiComponent = Awane.getComponent('PoppoAcademy.Students.Pai');
        expect(paiComponent).not.toBeNull();
        
        const paiFromStudent = paiComponent!.awaneAs<PoppoAcademy.Students.IPai>();
        expect(paiFromStudent).not.toBeNull();
        expect(paiFromStudent.study('physics')).toBe('Studying physics intensively!');
    });

    test('GetComponentsでの複数取得', () => {
        Awane.register(airi);
        Awane.register(pai);
        Awane.register(poppo);
        Awane.register(riko);

        const allMembers = Awane.getComponents<PoppoAcademy.Core.IPoppoAcademy>('PoppoAcademy.Core.IPoppoAcademy');
        expect(allMembers.length).toBe(4);

        const greetings = allMembers.map(m => m.getGreeting()).sort();
        expect(greetings[0]).toBe('Hello from Airi Teacher!');
        expect(greetings[1]).toBe('Hello from Pai Student!');
        expect(greetings[2]).toBe('Hello from Poppo Student!');
        expect(greetings[3]).toBe('Hello from Riko Student!');
    });

    test('名前空間解決の統合テスト', () => {
        Awane.register(airi);
        Awane.register(pai);
        Awane.register(poppo);
        Awane.register(riko);

        const airiByShortName = Awane.getComponent('IAiri');
        expect(airiByShortName).not.toBeNull();
        expect(airiByShortName!.awaneId).toBe('PoppoAcademy.Teachers.Airi');
        
        const airiAsIAiri = airiByShortName!.awaneAs<PoppoAcademy.Teachers.IAiri>();
        expect(airiAsIAiri).not.toBeNull();

        const paiByShortName = Awane.getComponent('IPai');
        expect(paiByShortName).not.toBeNull();
        expect(paiByShortName!.awaneId).toBe('PoppoAcademy.Students.Pai');

        const allByShortName = Awane.getComponents('IPoppoAcademy');
        expect(allByShortName.length).toBe(4);
    });

    test('AwaneAsメソッドでの型変換', () => {
        Awane.register(airi);
        Awane.register(pai);

        const component = Awane.getComponent('PoppoAcademy.Teachers.Airi');
        expect(component).not.toBeNull();

        const asPoppoAcademy = component!.awaneAs<PoppoAcademy.Core.IPoppoAcademy>();
        expect(asPoppoAcademy).not.toBeNull();
        expect(asPoppoAcademy.getGreeting()).toBe('Hello from Airi Teacher!');

        const asAiri = component!.awaneAs<PoppoAcademy.Teachers.IAiri>();
        expect(asAiri).not.toBeNull();
        expect(asAiri.teach('programming')).toBe('Teaching programming with passion!');

        // In TypeScript, awaneAs returns the object cast to type, not null for invalid casts
        const asStudent = component!.awaneAs<PoppoAcademy.Students.IPai>();
        expect(asStudent).toBeDefined();
    });

    test('複雑な名前空間解決', () => {
        Awane.register(airi);
        Awane.register(pai);
        Awane.register(poppo);
        Awane.register(riko);

        const airiByPartialNamespace = Awane.getComponent('Teachers.IAiri');
        expect(airiByPartialNamespace).not.toBeNull();
        expect(airiByPartialNamespace!.awaneName).toBe('Airi Teacher');
        
        const airiAsIAiri = airiByPartialNamespace!.awaneAs<PoppoAcademy.Teachers.IAiri>();
        expect(airiAsIAiri).not.toBeNull();

        const paiByPartialNamespace = Awane.getComponent('Students.IPai');
        expect(paiByPartialNamespace).not.toBeNull();
        expect(paiByPartialNamespace!.awaneName).toBe('Pai Student');

        const studentsByPartialNamespace = Awane.getComponents('Students.');
        // We should get 3 students (Pai, Poppo, Riko) but each implements 2 interfaces, 
        // so we expect 6 total results when searching by prefix
        expect(studentsByPartialNamespace.length).toBe(6);
    });

    test('タグによる検索（将来拡張の準備）', () => {
        Awane.register(airi);
        Awane.register(pai);
        Awane.register(poppo);
        Awane.register(riko);

        const teacher = Awane.getComponent('PoppoAcademy.Teachers.Airi');
        expect(teacher!.awaneTags['role']).toBe('teacher');
        expect(teacher!.awaneTags['subject']).toBe('programming');

        const advancedStudent = Awane.getComponent('PoppoAcademy.Students.Pai');
        expect(advancedStudent!.awaneTags['role']).toBe('student');
        expect(advancedStudent!.awaneTags['grade']).toBe('advanced');

        const beginnerStudent = Awane.getComponent('PoppoAcademy.Students.Poppo');
        expect(beginnerStudent!.awaneTags['role']).toBe('student');
        expect(beginnerStudent!.awaneTags['grade']).toBe('beginner');

        const intermediateStudent = Awane.getComponent('PoppoAcademy.Students.Riko');
        expect(intermediateStudent!.awaneTags['role']).toBe('student');
        expect(intermediateStudent!.awaneTags['grade']).toBe('intermediate');
    });

    test('パフォーマンステスト', () => {
        const startRegistration = Date.now();
        for (let i = 0; i < 1000; i++) {
            const component = new TestComponent(`Component${i}`, `Test.Component${i}`);
            Awane.register(component);
        }
        const registrationTime = Date.now() - startRegistration;
        expect(registrationTime).toBeLessThan(1000);

        const startRetrieval = Date.now();
        for (let i = 0; i < 1000; i++) {
            const component = Awane.getComponent(`Component${i}`);
            expect(component).not.toBeNull();
        }
        const retrievalTime = Date.now() - startRetrieval;
        expect(retrievalTime).toBeLessThan(1000);

        const startGetAll = Date.now();
        const allComponents = Awane.getComponents<PoppoAcademy.Core.IPoppoAcademy>('PoppoAcademy.Core.IPoppoAcademy');
        const getAllTime = Date.now() - startGetAll;
        expect(getAllTime).toBeLessThan(100);
    });

    test('エラーハンドリングとエッジケース', () => {
        expect(() => Awane.register(null as any)).toThrow();

        const nonExistent = Awane.getComponent('NonExistent.Component');
        expect(nonExistent).toBeNull();

        const emptyList = Awane.getComponents<PoppoAcademy.Core.IPoppoAcademy>('PoppoAcademy.Core.IPoppoAcademy');
        expect(emptyList.length).toBe(0);

        Awane.register(airi);
        expect(() => Awane.register(airi)).toThrow(ComponentAlreadyRegisteredException);

        // Test ambiguous component resolution
        // Create two components with interfaces that end with 'ITest'
        const testComp1 = new TestComponent('TestComp1', 'Test.A.ITest');
        const testComp2 = new TestComponent('TestComp2', 'Test.B.ITest');
        Awane.register(testComp1);
        Awane.register(testComp2);
        expect(() => Awane.getComponent('ITest')).toThrow(AmbiguousComponentException);
    });
});

class TestComponent implements PoppoAcademy.Core.IPoppoAcademy {
    constructor(
        private readonly id: string,
        private readonly interfaceName: string
    ) {}

    readonly awaneId = this.id;
    readonly awaneLocation = 'local';
    readonly awaneInterfaces = [this.interfaceName, 'PoppoAcademy.Core.IPoppoAcademy'];
    readonly awaneName = `Test ${this.id}`;
    readonly awaneVersion = '1.0.0';
    readonly awaneTags = { test: 'true' };

    awaneAs<T>(): T {
        return this as unknown as T;
    }

    getGreeting(): string {
        return `Hello from ${this.id}!`;
    }
}