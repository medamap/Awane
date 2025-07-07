import { randomUUID } from 'crypto';
import { IAwaneComponent } from './IAwaneComponent';

export abstract class AwaneComponent implements IAwaneComponent {
    private readonly _awaneId: string;
    private readonly _awaneInterfaces: string[];
    private readonly _awaneTags: Record<string, string>;
    
    public readonly awaneLocation: string;
    public readonly awaneName: string;
    public readonly awaneVersion: string;

    constructor() {
        this._awaneId = randomUUID();
        this.awaneLocation = 'process';
        this.awaneName = '';
        this.awaneVersion = '1.0.0';
        this._awaneTags = {};
        this._awaneInterfaces = [];
    }

    get awaneId(): string {
        return this._awaneId;
    }

    get awaneInterfaces(): string[] {
        return [...this._awaneInterfaces];
    }

    get awaneTags(): Record<string, string> {
        return { ...this._awaneTags };
    }

    public addInterface(interfaceName: string): void {
        if (!this._awaneInterfaces.includes(interfaceName)) {
            this._awaneInterfaces.push(interfaceName);
        }
    }

    public awaneAs<T>(): T {
        return this as unknown as T;
    }
}