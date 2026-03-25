using ABCShared.Library.Models.Requests.Schools;
using ABCShared.Library.Wrappers;
using Application.Pipelines;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Features.Schools.Commands;

public class CreateSchoolCommand : IRequest<IResponseWrapper>, IValidateMe
{
    public required CreateSchoolRequest CreateSchool { get; set; }
}

public class CreateSchoolCommandHandler(ISchoolService schoolService, IMapper mapper) : IRequestHandler<CreateSchoolCommand, IResponseWrapper>
{
    private readonly ISchoolService _schoolService = schoolService;
    private readonly IMapper _mapper = mapper;

    public async Task<IResponseWrapper> Handle(CreateSchoolCommand request, CancellationToken cancellationToken)
    {
        var newSchool = _mapper.Map<School>(request.CreateSchool);
        var schoolId = await _schoolService.CreateAsync(newSchool, cancellationToken);
        return ResponseWrapper<int>.Success(data: schoolId, message: "School created sucessfully");
    }
}