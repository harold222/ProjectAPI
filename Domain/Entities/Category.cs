namespace Domain.Entities;

public class Category
{
    public int CategoryId { get; private set; }
    public string CategoryName { get; private set; }
    public virtual ICollection<Post> Posts { get; set; }

    public static Category Create(string categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
            throw new ArgumentException("El nombre de categoría no puede estar vacío");
        if (categoryName.Length > 200)
            throw new ArgumentException("El nombre de categoría no puede exceder 200 caracteres");

        return new Category { CategoryName = categoryName };
    }
}
