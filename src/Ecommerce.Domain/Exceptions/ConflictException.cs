namespace Ecommerce.Domain.Exceptions;

public class ConflictException : Exception
{
    public ConflictException(string name, object key)
        : base($"Entity '{name}' with key ({key}) already exists.") { }

    public ConflictException(string message)
        : base(message) { }
}
