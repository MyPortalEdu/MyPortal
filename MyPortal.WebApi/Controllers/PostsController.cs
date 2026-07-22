using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models;
using MyPortal.Contracts.Models.People;
using MyPortal.Services.Interfaces.People;
using MyPortal.WebApi.Infrastructure.Attributes;

namespace MyPortal.WebApi.Controllers;

/// <summary>
/// Established posts — the school's staffing structure, maintained in Staff Setup. Contracts are
/// held against a post, and vacancies are reported for one.
/// </summary>
public sealed class PostsController(
    ProblemDetailsFactory problemFactory,
    ILogger<PostsController> logger,
    IPostService postService)
    : BaseApiController(problemFactory, logger)
{
    /// <summary>Get the posts register plus the option lists the editor needs.</summary>
    [HttpGet]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(PostsResponse), 200)]
    public async Task<IActionResult> GetPostsAsync()
    {
        var result = await postService.GetPostsAsync(CancellationToken);
        return Ok(result);
    }

    /// <summary>Create an established post.</summary>
    /// <param name="model">The post to create, with any vacancies.</param>
    [HttpPost]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> CreatePostAsync([FromBody] PostUpsertRequest model)
    {
        var id = await postService.CreatePostAsync(model, CancellationToken);
        return Ok(new IdResponse { Id = id });
    }

    /// <summary>Update an established post and its vacancies.</summary>
    /// <param name="postId">The Post id.</param>
    /// <param name="model">The new post payload.</param>
    [HttpPut("{postId:guid}")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> UpdatePostAsync([FromRoute] Guid postId,
        [FromBody] PostUpsertRequest model)
    {
        await postService.UpdatePostAsync(postId, model, CancellationToken);
        return Ok(new IdResponse { Id = postId });
    }

    /// <summary>Delete an established post. Rejected if any contract is held against it.</summary>
    /// <param name="postId">The Post id.</param>
    [HttpDelete("{postId:guid}")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> DeletePostAsync([FromRoute] Guid postId)
    {
        await postService.DeletePostAsync(postId, CancellationToken);
        return Ok(new IdResponse { Id = postId });
    }
}
