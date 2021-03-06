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

        public DbSet<ChecklistTemplate> ChecklistTemplates { get; set; }
        public DbSet<ChecklistTemplateItem> ChecklistTemplateItems { get; set; }
        public DbSet<Checklist> Checklists { get; set; }
        public DbSet<ChecklistItem> ChecklistItems { get; set; }
    }
}