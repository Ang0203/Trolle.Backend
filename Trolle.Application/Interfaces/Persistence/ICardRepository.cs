using System;
using System.Threading.Tasks;
using Trolle.Domain.Entities;

namespace Trolle.Application.Interfaces.Persistence;

public interface ICardRepository
{
    Task<Card?> GetByIdAsync(Guid id);
    Task AddAsync(Card card);
    Task UpdateAsync(Card card);
    Task DeleteAsync(Guid id);
}
