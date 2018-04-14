using Framework.Models;
using Framework.Repositories;
using Microsoft.EntityFrameworkCore;
using Checklists.Models;

namespace Checklists.Repositories
{
    public class ChecklistItemRepository : BaseRepository<ChecklistItem>
    {
        public ChecklistItemRepository(ChecklistsContext context) : base(context) { }

        protected override DbSet<ChecklistItem> _dbset
        {
            get
            {
                return ((ChecklistsContext)_context).ChecklistItems;
            }
        }
    }
}