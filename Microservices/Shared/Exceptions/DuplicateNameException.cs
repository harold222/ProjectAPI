namespace Shared.Exceptions;

/// <summary>
/// Thrown when a unique constraint is violated (e.g., duplicate Customer name).
/// Caught by BaseRepository.CreateOrThrowAsync / CreateAllOrThrowAsync
/// and translated from DbUpdateException (SQL error 2601/2627).
/// </summary>
public class DuplicateNameException : Exception
{
    public DuplicateNameException(string name)
        : base($"Ya existe un registro con el nombre '{name}'")
    {
    }
}
