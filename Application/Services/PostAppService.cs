using Application.DTOs;
using Domain;
using Domain.Entities;
using Domain.Exceptions;

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
        var (categoryId, categoryName) = await _categoryAppService.ResolveOrCreateAsync(dto.CategoryId, dto.CustomCategory);

        var entity = Post.Create(
            default,
            dto.Title?.Trim() ?? string.Empty,
            dto.Body,
            dto.CustomerId,
            categoryId
        );

        var created = await _postService.CreateOrThrowAsync(entity);
        return MapToResponse(created.entity);
    }

    public async Task<PostDto.CreateAllResult> CreateAllAsync(IEnumerable<PostDto.Create> dtos)
    {
        var result = new PostDto.CreateAllResult();

        var dtoList = dtos.ToList();
        if (dtoList.Count == 0)
            return result;

        foreach (var dto in dtoList)
        {
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
                Body = dto.Body,
                CategoryId = categoryId,
                CustomerId = dto.CustomerId
            });
        }

        if (result.Created.Count == 0)
            return result;

        var entitiesToCreate = result.Created.Select(p =>
            Post.Create(default, p.Title, p.Body, p.CustomerId, p.CategoryId)
        ).ToList();

        try
        {
            var created = await _postService.CreateAllOrThrowAsync(entitiesToCreate);

            for (int i = 0; i < result.Created.Count; i++)
            {
                result.Created[i].Id = created[i].PostId;
            }
        }
        catch (ForeignKeyViolationException)
        {
            result.Failed.AddRange(result.Created.Select(c => new PostDto.FailedItem
            {
                Title = c.Title,
                CustomerId = c.CustomerId,
                Reason = $"El usuario asociado (CustomerId: {c.CustomerId}) no existe"
            }));
            result.Created.Clear();
        }

        return result;
    }

    public async Task<PostDto.Response> UpdateAsync(PostDto.Update dto)
    {
        var existingPost = await _postService.GetAsync(dto.Id);

        if (existingPost == null)
            throw new KeyNotFoundException($"Post Id {dto.Id} no encontrado");

        int? categoryId = existingPost.CategoryId;
        string categoryName = existingPost.Category?.CategoryName ?? string.Empty;

        (categoryId, categoryName) = await _categoryAppService.ResolveOrCreateAsync(dto.CategoryId, dto.CustomCategory);

        var entity = Post.Create(
            dto.Id,
            dto.Title?.Trim() ?? string.Empty,
            dto.Body,
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
}
