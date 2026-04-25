namespace Domain.Entities;

public class Category
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
    public virtual ICollection<Post> Posts { get; set; }
}