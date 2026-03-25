using ABCShared.Library.Constants;
using ABCShared.Library.Models.Requests.Identity;
using Application.Features.Identity.Users.Commands;
using Application.Features.Identity.Users.Queries;
using Infrastructure.Identity.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Route("api/[controller]")]
public class UsersController : BaseApiController
{
    [HttpPost("register")]
    [ShouldHavePermission(action: SchoolAction.Create, feature: SchoolFeature.Users)]
    public async Task<IActionResult> RegisterUserAsync(CreateUserRequest createUser)
    {
        var response = await Sender.Send(new CreateUserCommand { CreateUser = createUser });
        if (response.IsSuccessful) return Ok(response);
        return BadRequest(response);
    }

    [HttpPut("update")]
    [ShouldHavePermission(action: SchoolAction.Update, feature: SchoolFeature.Users)]
    public async Task<IActionResult> UpdateUSerAsync(UpdateUserRequest updateUser)
    {
        var response = await Sender.Send(new UpdateUserCommand { UpdateUser = updateUser });
        if (response.IsSuccessful) return Ok(response);
        return NotFound(response);
    }

    [HttpPut("update-status")]
    [ShouldHavePermission(action: SchoolAction.Update, feature: SchoolFeature.Users)]
    public async Task<IActionResult> ChangeStatusAsync(ChangeUserStatusRequest changeUserStatus)
    {
        var response = await Sender.Send(new UpdateUserStatusCommand { ChangeUserStatus = changeUserStatus });
        if (response.IsSuccessful) return Ok(response);
        return NotFound(response);
    }

    [HttpPut("update-roles/{roleId}")]
    [ShouldHavePermission(action: SchoolAction.Update, feature: SchoolFeature.UserRoles)]
    public async Task<IActionResult> UpdateUserRolesAsync(UserRolesRequest userRolesRequest, string roleId)
    {
        var response = await Sender.Send(new UpdateUserRolesCommand { UserRolesRequest = userRolesRequest, RoleId = roleId });
        if (response.IsSuccessful) return Ok(response);
        return NotFound(response);
    }

    [HttpDelete("delete/{userId}")]
    [ShouldHavePermission(action: SchoolAction.Delete, feature: SchoolFeature.Users)]
    public async Task<IActionResult> DeleteUserAsync(string userId)
    {
        var response = await Sender.Send(new DeleteUserCommand { UserId = userId });
        if (response.IsSuccessful) return Ok(response);
        return NotFound(response);
    }

    [HttpGet("all")]
    [ShouldHavePermission(action: SchoolAction.Read, feature: SchoolFeature.Users)]
    public async Task<IActionResult> GetAllUsersAsync()
    {
        var response = await Sender.Send(new GetAllUsersQuery { });
        if (response.IsSuccessful) return Ok(response);
        return NotFound(response);
    }

    [HttpGet("permissions/{userId}")]
    [ShouldHavePermission(action: SchoolAction.Read, feature: SchoolFeature.RoleClaims)]
    public async Task<IActionResult> GetUserPermissionsAsync(string userId)
    {
        var response = await Sender.Send(new UserPermissionsQuery { UserId = userId });
        if (response.IsSuccessful) return Ok(response);
        return NotFound(response);
    }

    [HttpGet("user-roles/{userId}")]
    [ShouldHavePermission(action: SchoolAction.Read, feature: SchoolFeature.UserRoles)]
    public async Task<IActionResult> GetUSerRolesAsync(string userId)
    {
        var response = await Sender.Send(new GetUserRolesQuery { UserId = userId });
        if (response.IsSuccessful) return Ok(response);
        return NotFound(response);
    }

    [HttpPut("change-password")]
    [AllowAnonymous]
    public async Task<IActionResult> CHangeUserPasswordAsync(ChangePasswordRequest changePassword)
    {
        var response = await Sender.Send(new ChangeUserPasswordCommand { ChangePassword = changePassword });
        if (response.IsSuccessful) return Ok(response);
        return BadRequest(response);
    }
}
