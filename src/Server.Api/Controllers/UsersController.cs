using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Server.Api.Contracts.Requests;
using Server.Api.Contracts.Responses;
using Server.Api.Data.Entities;
using Server.Api.Exceptions;
using Server.Api.Infrastructure;
using Server.Api.Services;

namespace Server.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = AppRoles.Admin)]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await _userService.GetAllAsync(cancellationToken);
        return Ok(users.Select(MapUser).ToArray());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(MapUser(user));
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserResponse>> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var created = await _userService.CreateAsync(request, cancellationToken);
            var response = MapUser(created);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (ValidationProblemException exception)
        {
            return ValidationProblem(ToModelState(exception));
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> Update(
        int id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var updated = await _userService.UpdateAsync(id, request, cancellationToken);
            if (updated is null)
            {
                return NotFound();
            }

            return Ok(MapUser(updated));
        }
        catch (ValidationProblemException exception)
        {
            return ValidationProblem(ToModelState(exception));
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _userService.DeleteAsync(id, cancellationToken);
        return result switch
        {
            DeleteUserResult.NotFound => NotFound(),
            DeleteUserResult.SystemAccountProtected => BadRequest(new ProblemDetails
            {
                Title = "Protected user",
                Detail = "System accounts cannot be deleted.",
                Status = StatusCodes.Status400BadRequest
            }),
            _ => NoContent()
        };
    }

    private static UserResponse MapUser(UserEntity user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            IsSystemAccount = user.IsSystemAccount,
            CreatedAtUtc = user.CreatedAtUtc,
            UpdatedAtUtc = user.UpdatedAtUtc
        };
    }

    private static ModelStateDictionary ToModelState(ValidationProblemException exception)
    {
        var modelState = new ModelStateDictionary();
        foreach (var entry in exception.Errors)
        {
            foreach (var error in entry.Value)
            {
                modelState.AddModelError(entry.Key, error);
            }
        }

        return modelState;
    }
}
