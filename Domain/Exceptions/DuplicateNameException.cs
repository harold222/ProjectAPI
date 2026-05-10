namespace Domain.Exceptions;

public class DuplicateNameException : Exception
{
    public DuplicateNameException(string name)
        : base($"Ya existe un registro con el nombre '{name}'")
    {
    }
}
