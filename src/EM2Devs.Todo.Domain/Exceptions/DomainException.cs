namespace EM2Devs.Todo.Domain.Exceptions;

public sealed class DomainException(string message) : Exception(message);
