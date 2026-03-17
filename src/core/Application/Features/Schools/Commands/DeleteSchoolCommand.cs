using Application.Wrappers;
using MediatR;

namespace Application.Features.Schools.Commands;

public class DeleteSchoolCommand : IRequest<IResponseWrapper>
{
    public int SchoolId { get; set; }
}

public class DeleteSchoolCommandHandler(ISchoolService schoolService) : IRequestHandler<DeleteSchoolCommand, IResponseWrapper>
{
    private readonly ISchoolService _schoolService = schoolService;

    public async Task<IResponseWrapper> Handle(DeleteSchoolCommand request, CancellationToken cancellationToken)
    {
        var schoolInDb = await _schoolService.GetByIdAsync(request.SchoolId, cancellationToken);
        if (schoolInDb is not null)
        {
            var deletedSchoolId = await _schoolService.DeleteAsync(schoolInDb, cancellationToken);
            return ResponseWrapper<int>.Success(data: deletedSchoolId, message: "School deleted successfully");
        }
        return ResponseWrapper<int>.Fail(message: "Failed to find school");
    }
}