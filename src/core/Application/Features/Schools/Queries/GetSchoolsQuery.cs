using ABCShared.Library.Models.Responses.Schools;
using ABCShared.Library.Wrappers;
using AutoMapper;
using MediatR;

namespace Application.Features.Schools.Queries;

public class GetSchoolsQuery : IRequest<IResponseWrapper> { }

public class GetSchoolsQueryHandler(ISchoolService schoolService, IMapper mapper) : IRequestHandler<GetSchoolsQuery, IResponseWrapper>
{
    private readonly IMapper _mapper = mapper;
    private readonly ISchoolService _schoolService = schoolService;

    public async Task<IResponseWrapper> Handle(GetSchoolsQuery request, CancellationToken cancellationToken)
    {
        var schoolsInDb = await _schoolService.GetAllAsync(cancellationToken);
        if (schoolsInDb.Count > 0)
        {
            return ResponseWrapper<List<SchoolResponse>>.Success(data: _mapper.Map<List<SchoolResponse>>(schoolsInDb));
        }
        return ResponseWrapper<List<SchoolResponse>>.Fail("No schools were found");
    }
}