using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Posts;

[Route("[controller]")]
public class PostController : ControllerBase
{
    private readonly PostAppService _postService;

    public PostController(PostAppService postService)
    {
        _postService = postService;
    }

    [HttpGet()]
    public async Task<ActionResult<IEnumerable<PostDto.Response>>> GetAll() => Ok(await _postService.GetAllAsync());

    [HttpPost()]
    public async Task<ActionResult<PostDto.Response>> Create([FromBody] PostDto.Create dto) => CreatedAtAction(nameof(GetAll), await _postService.CreateAsync(dto));

    [HttpPost("CreateAll")]
    public async Task<ActionResult<PostDto.CreateAllResult>> CreateAll([FromBody] IEnumerable<PostDto.Create> dtos) =>  Ok(await _postService.CreateAllAsync(dtos));

    [HttpPut()]
    public async Task<ActionResult<PostDto.Response>> Update([FromBody] PostDto.Update dto) => Ok(await _postService.UpdateAsync(dto));

    [HttpDelete("{id}")]
    public async Task<ActionResult<PostDto.DeleteResponse>> Delete(int id) => Ok(await _postService.DeleteAsync(id));
}