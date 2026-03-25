using ABCShared.Library.Models.Responses.Schools;
using ABCShared.Library.Wrappers;
using AutoMapper;
using MediatR;

namespace Application.Features.Schools.Queries;

public class GetSchoolByIdQuery : IRequest<IResponseWrapper>
{
    public int SchoolId { get; set; }
}

public class GetSchoolByIdQueryHandler(ISchoolService schoolService, IMapper mapper) : IRequestHandler<GetSchoolByIdQuery, IResponseWrapper>
{
    private readonly IMapper _mapper = mapper;
    private readonly ISchoolService _schoolService = schoolService;

    public async Task<IResponseWrapper> Handle(GetSchoolByIdQuery request, CancellationToken cancellationToken)
    {
        var schoolInDb = await _schoolService.GetByIdAsync(request.SchoolId);
        if (schoolInDb is not null)
        {
            return ResponseWrapper<SchoolResponse>.Success(data: _mapper.Map<SchoolResponse>(schoolInDb));
        }
        return ResponseWrapper<SchoolResponse>.Fail("School does not exist");
    }
}