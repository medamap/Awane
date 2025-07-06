export interface IAwaneComponent {
    readonly awaneId: string;
    
    readonly awaneLocation: string;
    
    readonly awaneInterfaces: string[];
    
    readonly awaneName: string;
    
    readonly awaneVersion: string;
    
    readonly awaneTags: Record<string, string>;
    
    awaneAs<T>(): T;
}