namespace Infrastructure.Tenancy;

public interface ITenantDbSeeder
{
    Task InitialiseDatabaseAsync(CancellationToken cancellationToken);
}
