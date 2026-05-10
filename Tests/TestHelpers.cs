using Domain.Entities;
using System.Reflection;

namespace Tests;

public static class TestHelpers
{
    public static Category CreateCategory(int id, string name)
    {
        var category = Category.Create(name);
        typeof(Category).GetProperty(nameof(Category.CategoryId), BindingFlags.Public | BindingFlags.Instance)!
            .SetValue(category, id);
        return category;
    }

    public static Customer CreateCustomer(int id, string name)
    {
        var customer = Customer.Create(id, name);
        return customer;
    }

    public static Post CreatePost(int postId, string title, string body, int customerId, int? categoryId = null)
    {
        return Post.Create(postId, title, body, customerId, categoryId);
    }
}
