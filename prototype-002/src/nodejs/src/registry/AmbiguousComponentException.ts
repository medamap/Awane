export class AmbiguousComponentException extends Error {
    constructor(message: string) {
        super(message);
        this.name = 'AmbiguousComponentException';
    }
}