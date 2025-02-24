using MemoryPack;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using ActualLab.Fusion;
using ActualLab.CommandR.Configuration;
using System.Collections.Immutable;
using ActualLab.CommandR;

namespace Abstractions;

// Entity
[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public partial class Country
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [DataMember, MemoryPackOrder(0)] public long Id { get; set; }
    [Required, MaxLength(4000)]
    [DataMember, MemoryPackOrder(1)] public string Name { get; set; } = "";
}

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public partial record CountryList
{
    [DataMember, MemoryPackOrder(0)] public ImmutableArray<Country> Countries { get; init; } = ImmutableArray<Country>.Empty;
}

public interface ICountryService : IComputeService
{
    [CommandHandler]
    Task<bool> EditAsync(EditCountryCommand command, CancellationToken cancellationToken = default);

    [ComputeMethod(AutoInvalidationDelay = 3)]
    Task<CountryList> ListAsync(int count, CancellationToken cancellationToken = default);

    [ComputeMethod]
    Task<Country?> GetAsync(long id, CancellationToken cancellationToken = default);
}

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public partial record EditCountryCommand(
        [property: DataMember, MemoryPackOrder(0)] long Id,
        [property: DataMember, MemoryPackOrder(1)] Country? CountryDto
) : ICommand<bool>;
