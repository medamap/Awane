import { IAwaneComponent } from '../../src/components/IAwaneComponent';

export namespace PoppoAcademy {
    export namespace Core {
        export interface IPoppoAcademy extends IAwaneComponent {
            getGreeting(): string;
        }
    }

    export namespace Teachers {
        export interface IAiri extends IAwaneComponent {
            teach(topic: string): string;
        }
    }

    export namespace Students {
        export interface IPai extends IAwaneComponent {
            study(subject: string): string;
        }

        export interface IPoppo extends IAwaneComponent {
            learn(lesson: string): string;
        }

        export interface IRiko extends IAwaneComponent {
            practice(skill: string): string;
        }
    }
}

export class Airi implements PoppoAcademy.Core.IPoppoAcademy, PoppoAcademy.Teachers.IAiri {
    readonly awaneId = 'PoppoAcademy.Teachers.Airi';
    readonly awaneLocation = 'local';
    readonly awaneInterfaces = [
        'PoppoAcademy.Core.IPoppoAcademy',
        'PoppoAcademy.Teachers.IAiri'
    ];
    readonly awaneName = 'Airi Teacher';
    readonly awaneVersion = '2.0.0';
    readonly awaneTags = {
        role: 'teacher',
        subject: 'programming'
    };

    awaneAs<T>(): T {
        return this as unknown as T;
    }

    getGreeting(): string {
        return 'Hello from Airi Teacher!';
    }

    teach(topic: string): string {
        return `Teaching ${topic} with passion!`;
    }
}

export class Pai implements PoppoAcademy.Core.IPoppoAcademy, PoppoAcademy.Students.IPai {
    readonly awaneId = 'PoppoAcademy.Students.Pai';
    readonly awaneLocation = 'local';
    readonly awaneInterfaces = [
        'PoppoAcademy.Core.IPoppoAcademy',
        'PoppoAcademy.Students.IPai'
    ];
    readonly awaneName = 'Pai Student';
    readonly awaneVersion = '1.0.0';
    readonly awaneTags = {
        role: 'student',
        grade: 'advanced'
    };

    awaneAs<T>(): T {
        return this as unknown as T;
    }

    getGreeting(): string {
        return 'Hello from Pai Student!';
    }

    study(subject: string): string {
        return `Studying ${subject} intensively!`;
    }
}

export class Poppo implements PoppoAcademy.Core.IPoppoAcademy, PoppoAcademy.Students.IPoppo {
    readonly awaneId = 'PoppoAcademy.Students.Poppo';
    readonly awaneLocation = 'local';
    readonly awaneInterfaces = [
        'PoppoAcademy.Core.IPoppoAcademy',
        'PoppoAcademy.Students.IPoppo'
    ];
    readonly awaneName = 'Poppo Student';
    readonly awaneVersion = '1.0.0';
    readonly awaneTags = {
        role: 'student',
        grade: 'beginner'
    };

    awaneAs<T>(): T {
        return this as unknown as T;
    }

    getGreeting(): string {
        return 'Hello from Poppo Student!';
    }

    learn(lesson: string): string {
        return `Learning ${lesson} step by step!`;
    }
}

export class Riko implements PoppoAcademy.Core.IPoppoAcademy, PoppoAcademy.Students.IRiko {
    readonly awaneId = 'PoppoAcademy.Students.Riko';
    readonly awaneLocation = 'local';
    readonly awaneInterfaces = [
        'PoppoAcademy.Core.IPoppoAcademy',
        'PoppoAcademy.Students.IRiko'
    ];
    readonly awaneName = 'Riko Student';
    readonly awaneVersion = '1.5.0';
    readonly awaneTags = {
        role: 'student',
        grade: 'intermediate'
    };

    awaneAs<T>(): T {
        return this as unknown as T;
    }

    getGreeting(): string {
        return 'Hello from Riko Student!';
    }

    practice(skill: string): string {
        return `Practicing ${skill} diligently!`;
    }
}