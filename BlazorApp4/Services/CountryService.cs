using Abstractions;
using ActualLab.CommandR.Configuration;
using ActualLab.Fusion;
using ActualLab.Fusion.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp4.Services;

public class CountryService(IServiceProvider services) : DbServiceBase<AppDbContext>(services), ICountryService
{
    [CommandHandler]
    public virtual async Task<bool> EditAsync(EditCountryCommand command, CancellationToken cancellationToken = default)
    {
        var (countryId, country) = command;

        if (countryId == 0)
            throw new ArgumentOutOfRangeException(nameof(command));

        if (Invalidation.IsActive)
        {
            _ = GetAsync(countryId, default);
            return false;
        }

        await using var dbContext = await DbHub.CreateOperationDbContext(cancellationToken);
        var dbCountry = await dbContext.Countries.FindAsync(DbKey.Compose(countryId), cancellationToken);

        if (country == null)
        {
            if (dbCountry != null)
                dbContext.Remove(dbCountry);
        }
        else
        {
            if (dbCountry != null)
            {
                dbCountry.Name = country.Name;
            }
            else
                dbContext.Add(new Country { Name = country.Name });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        /*
        // Adding LogMessageCommand as event
        var context = CommandContext.GetCurrent();
        var message = product == null
            ? $"Product removed: {countryId}"
            : $"Product updated: {countryId} with Code = {product.Code}, Name = {product.Name}";
        var logEvent = new LogMessageCommand(Ulid.NewUlid().ToString(), message);
        context.Operation.AddEvent(logEvent, logEvent.Uuid);
        var randomEvent = LogMessageCommand.New();
        context.Operation.AddEvent(randomEvent, randomEvent.Uuid);
        */

        return true;
    }

    public virtual async Task<CountryList> ListAsync(int count, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await DbHub.CreateDbContext(cancellationToken);
        var dbCountries = await dbContext.Countries.ToListAsync(cancellationToken);
        var countries = dbCountries.Select(dbCountry => new Country
            { Id = dbCountry.Id, Name = dbCountry.Name }).ToList();

        return new CountryList { Countries = [.. countries] };
    }

    public virtual async Task<Country?> GetAsync(long id, CancellationToken cancellationToken = default)
    {
        var countryResolver = services.DbEntityResolver<long, Country>();
        var dbCountry = await countryResolver.Get(id, cancellationToken);

        return dbCountry == null ? null : new Country { Id = dbCountry.Id, Name = dbCountry.Name };
    }

}
