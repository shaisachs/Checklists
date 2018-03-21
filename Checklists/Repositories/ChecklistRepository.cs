using Framework.Models;
using Framework.Repositories;
using Microsoft.EntityFrameworkCore;
using Checklists.Models;

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