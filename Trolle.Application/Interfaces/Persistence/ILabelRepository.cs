using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trolle.Domain.Entities;

namespace Trolle.Application.Interfaces.Persistence;

public interface ILabelRepository
{
    Task<Label?> GetByIdAsync(Guid id);
    Task<IEnumerable<Label>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task AddAsync(Label label);
    Task UpdateAsync(Label label);
    Task DeleteAsync(Guid id);
}
