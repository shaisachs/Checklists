using Microsoft.EntityFrameworkCore;
using Checklists.Models;

namespace Checklists.Repositories
{
    public class ChecklistsContext : DbContext
    {
        public ChecklistsContext(DbContextOptions<ChecklistsContext> options)
            : base(options)
        {
        }

        public DbSet<Checklist> Checklists { get; set; }
    }
}