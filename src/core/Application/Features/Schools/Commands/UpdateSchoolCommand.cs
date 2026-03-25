using ABCShared.Library.Models.Requests.Schools;
using ABCShared.Library.Wrappers;
using Application.Pipelines;
using MediatR;

namespace Application.Features.Schools.Commands;

public class UpdateSchoolCommand : IRequest<IResponseWrapper>, IValidateMe
{
    public required UpdateSchoolRequest UpdateSchool { get; set; }
}

public class UpdateSchoolCommandHandler(ISchoolService schoolService) : IRequestHandler<UpdateSchoolCommand, IResponseWrapper>
{
    private readonly ISchoolService _schoolService = schoolService;

    public async Task<IResponseWrapper> Handle(UpdateSchoolCommand request, CancellationToken cancellationToken)
    {
        var schoolInDb = await _schoolService.GetByIdAsync(request.UpdateSchool.Id);
        if (schoolInDb is not null)
        {
            schoolInDb.Name = request.UpdateSchool.Name;
            schoolInDb.EstablishedDate = request.UpdateSchool.EstablishedDate;
            var updatedSchoolId = await _schoolService.UpdateAsync(schoolInDb, cancellationToken);
            return ResponseWrapper<int>.Success(data: updatedSchoolId, message: "School updated successfully");
        }
        return ResponseWrapper<int>.Fail(message: "School not found");
    }
}