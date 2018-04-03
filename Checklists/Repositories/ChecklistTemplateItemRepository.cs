using Framework.Models;
using Framework.Repositories;
using Microsoft.EntityFrameworkCore;
using Checklists.Models;

namespace Checklists.Repositories
{
    public class ChecklistTemplateItemRepository : BaseRepository<ChecklistTemplateItem>
    {
        public ChecklistTemplateItemRepository(ChecklistsContext context) : base(context) { }

        protected override DbSet<ChecklistTemplateItem> _dbset
        {
            get
            {
                return ((ChecklistsContext)_context).ChecklistTemplateItems;
            }
        }
    }
}