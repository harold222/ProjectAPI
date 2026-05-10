using Application.DTOs;
using Domain.Entities;

namespace Application.Services;

public class PostAppService
{
    private readonly BaseService<Post> _postService;
    private readonly BaseService<Customer> _customerService;
    private readonly CategoryAppService _categoryAppService;

    public PostAppService(
        BaseService<Post> postService,
        BaseService<Customer> customerService,
        CategoryAppService categoryAppService
    )
    {
        _postService = postService;
        _customerService = customerService;
        _categoryAppService = categoryAppService;
    }

    public async Task<IEnumerable<PostDto.Response>> GetAllAsync()
    {
        var posts = await _postService.GetAllAsync();
        return posts.Select(MapToResponse);
    }

    public async Task<PostDto.Response> CreateAsync(PostDto.Create dto)
    {
        var customer = await _customerService.GetAsync(dto.CustomerId);

        if (customer == null)
            throw new InvalidOperationException("El usuario asociado no existe");

        var (categoryId, categoryName) = await _categoryAppService.ResolveOrCreateAsync(dto.CategoryId, dto.CustomCategory);

        var entity = Post.Create(
            default,
            dto.Title?.Trim() ?? string.Empty,
            FormatBody(dto.Body),
            dto.CustomerId,
            categoryId
        );

        var created = await _postService.CreateAsync(entity);

        return new PostDto.Response
        {
            Id = created.PostId,
            Title = created.Title,
            Body = created.Body,
            CategoryId = categoryId,
            CustomerId = created.CustomerId
        };
    }

    public async Task<PostDto.CreateAllResult> CreateAllAsync(IEnumerable<PostDto.Create> dtos)
    {
        var result = new PostDto.CreateAllResult();

        var dtoList = dtos.ToList();
        if (dtoList.Count == 0)
            return result;

        var existingCustomers = await _customerService.GetAllAsync();
        var existingCustomerIds = existingCustomers.Select(c => c.CustomerId).ToHashSet();

        foreach (var dto in dtoList)
        {
            if (!existingCustomerIds.Contains(dto.CustomerId))
            {
                result.Failed.Add(new PostDto.FailedItem
                {
                    Title = dto.Title,
                    CustomerId = dto.CustomerId,
                    Reason = "El usuario asociado no existe"
                });
                continue;
            }

            int? categoryId;
            string categoryName;

            try
            {
                (categoryId, categoryName) = await _categoryAppService.ResolveOrCreateAsync(dto.CategoryId, dto.CustomCategory);
            }
            catch (InvalidOperationException ex)
            {
                result.Failed.Add(new PostDto.FailedItem
                {
                    Title = dto.Title,
                    CustomerId = dto.CustomerId,
                    Reason = ex.Message
                });
                continue;
            }

            result.Created.Add(new PostDto.Response
            {
                Id = 0,
                Title = dto.Title?.Trim() ?? string.Empty,
                Body = FormatBody(dto.Body),
                CategoryId = categoryId,
                CustomerId = dto.CustomerId
            });
        }

        if (result.Created.Count == 0)
            return result;

        var entitiesToCreate = result.Created.Select(p => 
            Post.Create(
                default,
                p.Title,
                p.Body,
                p.CustomerId,
                p.CategoryId
            )
        ).ToList();

        var created = await _postService.CreateAllAsync(entitiesToCreate);

        for (int i = 0; i < result.Created.Count; i++)
        {
            result.Created[i].Id = created[i].PostId;
        }

        return result;
    }

    public async Task<PostDto.Response> UpdateAsync(PostDto.Update dto)
    {
        var existingPost = await _postService.GetAsync(dto.Id);

        if (existingPost == null)
            throw new KeyNotFoundException($"Post Id {dto.Id} no encontrado");

        var body = FormatBody(dto.Body);

        int? categoryId = existingPost.CategoryId;
        string categoryName = existingPost.Category?.CategoryName ?? string.Empty;

        (categoryId, categoryName) = await _categoryAppService.ResolveOrCreateAsync(dto.CategoryId, dto.CustomCategory);

        var entity = Post.Create(
            dto.Id,
            dto.Title?.Trim() ?? string.Empty,
            body,
            existingPost.CustomerId,
            categoryId
        );

        var (updated, changed) = await _postService.UpdateAsync(dto.Id, entity);

        return MapToResponse(updated);
    }

    public async Task<PostDto.DeleteResponse> DeleteAsync(int id)
    {
        var post = await _postService.GetAsync(id);

        if (post == null)
            return new() { Status = false };

        await _postService.DeleteAsync(post);
        return new() { Status = true };
    }

    private static PostDto.Response MapToResponse(Post post) => new PostDto.Response
    {
        Id = post.PostId,
        Title = post.Title,
        Body = post.Body,
        CategoryId = post.CategoryId,
        CustomerId = post.CustomerId
    };

    private static string FormatBody(string? body)
    {
        var trimmed = body?.Trim() ?? string.Empty;

        if (trimmed.Length <= 20)
            return trimmed;

        return trimmed.Substring(0, 97) + "...";
    }
}
