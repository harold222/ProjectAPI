namespace PostService.Domain.Entities;

/// <summary>
/// Post aggregate root — standalone entity in PostService.
/// CustomerId references an external service (CustomerService), no navigation property here.
/// </summary>
public class Post
{
    public int PostId { get; private set; }
    public string Title { get; private set; }
    public string Body { get; private set; }
    public int? CategoryId { get; private set; }
    public int CustomerId { get; private set; }

    /// <summary>
    /// Navigation to Category — still local since Category lives in PostService's own DB.
    /// </summary>
    public virtual Category Category { get; set; }

    /// <summary>
    /// Static factory with validation. CustomerId existence is validated externally
    /// via ICustomerServiceClient (HTTP call to CustomerService) before this is called.
    /// </summary>
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
