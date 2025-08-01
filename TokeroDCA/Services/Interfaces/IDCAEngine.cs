using TokeroDCA.Models;

namespace TokeroDCA.Services.Interfaces;

public interface IDCAEngine
{
    Task<List<DCAEvent>> CalculateDCAAsync(DCASetup setup);
}