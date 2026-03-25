using ABCShared.Library.Models.Responses.Schools;
using ABCShared.Library.Wrappers;
using AutoMapper;
using MediatR;

namespace Application.Features.Schools.Queries;

public class GetSchoolByNameQuery : IRequest<IResponseWrapper>
{
    public required string Name { get; set; }
}

public class GetSchoolByNameQueryHandler(IMapper mapper, ISchoolService schoolService) : IRequestHandler<GetSchoolByNameQuery, IResponseWrapper>
{
    private readonly ISchoolService _schoolService = schoolService;
    private readonly IMapper _mapper = mapper;

    public async Task<IResponseWrapper> Handle(GetSchoolByNameQuery request, CancellationToken cancellationToken)
    {
        var schoolInDb = await _schoolService.GetByNameAsync(request.Name, cancellationToken);
        if (schoolInDb is not null)
        {
            return ResponseWrapper<SchoolResponse>.Success(data: _mapper.Map<SchoolResponse>(schoolInDb));
        }
        return ResponseWrapper<SchoolResponse>.Fail("School does not exist");
    }
}