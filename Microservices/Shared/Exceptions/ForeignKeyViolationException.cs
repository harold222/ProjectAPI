namespace Shared.Exceptions;

/// <summary>
/// Thrown when a foreign key constraint is violated.
/// Caught by BaseRepository.CreateOrThrowAsync / CreateAllOrThrowAsync
/// and translated from DbUpdateException (SQL error 547).
/// </summary>
public class ForeignKeyViolationException : Exception
{
    public string ReferencedEntity { get; }

    public ForeignKeyViolationException(string referencedEntity)
        : base($"Error referencia en '{referencedEntity}'")
    {
        ReferencedEntity = referencedEntity;
    }
}
