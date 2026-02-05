using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Trolle.Application.Interfaces.Persistence;
using Trolle.Domain.Entities;
using Trolle.Infrastructure.Persistence;

namespace Trolle.Infrastructure.Repositories;

public class LabelRepository : ILabelRepository
{
    private readonly TrolleDbContext _context;

    public LabelRepository(TrolleDbContext context)
    {
        _context = context;
    }

    public async Task<Label?> GetByIdAsync(Guid id)
    {
        return await _context.Labels.FindAsync(id);
    }

    public async Task<IEnumerable<Label>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        return await _context.Labels
            .Where(l => ids.Contains(l.Id))
            .ToListAsync();
    }

    public async Task AddAsync(Label label)
    {
        await _context.Labels.AddAsync(label);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Label label)
    {
        _context.Labels.Update(label);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var label = await _context.Labels.FindAsync(id);
        if (label != null)
        {
            _context.Labels.Remove(label);
            await _context.SaveChangesAsync();
        }
    }
}
