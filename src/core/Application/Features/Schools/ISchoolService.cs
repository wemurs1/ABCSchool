using Domain.Entities;

namespace Application.Features.Schools;

public interface ISchoolService
{
    Task<int> CreateAsync(School school, CancellationToken ct = default);
    Task<int> UpdateAsync(School school, CancellationToken ct = default);
    Task<int> DeleteAsync(School school, CancellationToken ct = default);
    Task<School?> GetByIdAsync(int schoolId, CancellationToken ct = default);
    Task<List<School>> GetAllAsync(CancellationToken ct = default);
    Task<School?> GetByNameAsync(string name, CancellationToken ct = default);
}
