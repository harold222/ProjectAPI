namespace Domain.Entities;

public class Post
{
    public int PostId { get; private set; }
    public string Title { get; private set; }
    public string Body { get; private set; }
    public int? CategoryId { get; private set; }
    public int CustomerId { get; private set; }
    public virtual Customer Customer { get; set; }
    public virtual Category Category { get; set; }

    public static Post Create(int postId, string title, string body, int customerId, int? categoryId = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("El título no puede estar vacío");

        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("El body no puede estar vacío");

        if (customerId <= 0)
            throw new ArgumentException("El customerId debe ser mayor a 0");


        var fixBody = body.Trim();

        if (fixBody.Length > 20)
            fixBody = fixBody.Substring(0, 97) + "...";

        return new Post
        {
            PostId = postId,
            Title = title,
            Body = fixBody,
            CustomerId = customerId,
            CategoryId = categoryId
        };
    }
}
