namespace Domain.Exceptions;

public class ForeignKeyViolationException : Exception
{
    public string ReferencedEntity { get; }

    public ForeignKeyViolationException(string referencedEntity)
        : base($"Error referencia en '{referencedEntity}'")
    {
        ReferencedEntity = referencedEntity;
    }
}
