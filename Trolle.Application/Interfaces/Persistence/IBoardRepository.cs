using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trolle.Domain.Entities;

namespace Trolle.Application.Interfaces.Persistence;

public interface IBoardRepository
{
    Task<Board?> GetByIdAsync(Guid id);
    Task<IEnumerable<Board>> GetAllAsync();
    Task AddAsync(Board board);
    Task UpdateAsync(Board board);
    Task DeleteAsync(Guid id);
    
    // For specialized loading
    Task<Board?> GetByIdWithDetailsAsync(Guid id);
}
