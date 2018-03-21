using Checklists.Models;
using Microsoft.EntityFrameworkCore;

namespace Checklists.Repositories
{
    public class ChecklistRepository : BaseRepository<Checklist>
    {
        public ChecklistRepository(ChecklistsContext context) : base(context) { }

        protected override DbSet<Checklist> _dbset
        {
            get
            {
                return ((ChecklistsContext)_context).Checklists;
            }
        }
    }
}