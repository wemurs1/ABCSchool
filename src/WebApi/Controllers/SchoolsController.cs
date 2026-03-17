using Application.Features.Schools;
using Application.Features.Schools.Commands;
using Infrastructure.Constants;
using Infrastructure.Identity.Auth;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    public class SchoolsController : BaseApiController
    {
        [HttpPost("add")]
        [ShouldHavePermission(action: SchoolAction.Create, feature: SchoolFeature.Schools)]
        public async Task<IActionResult> CreateSchoolAsync(CreateSchoolRequest createSchool)
        {
            var response = await Sender.Send(new CreateSchoolCommand { CreateSchool = createSchool });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpPut("update")]
        [ShouldHavePermission(action: SchoolAction.Update, feature: SchoolFeature.Schools)]
        public async Task<IActionResult> UpdateSchoolAsync(UpdateSchoolRequest updateSchool)
        {
            var response = await Sender.Send(new UpdateSchoolCommand { UpdateSchool = updateSchool });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return NotFound(response);
        }

        [HttpDelete("{schoolId}")]
        [ShouldHavePermission(action: SchoolAction.Delete, feature: SchoolFeature.Schools)]
        public async Task<IActionResult> DeleteSchoolAsync(int schoolId)
        {
            var response = await Sender.Send(new DeleteSchoolCommand { SchoolId = schoolId });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return NotFound(response);
        }
    }
}
