using Framework.Models;
using Framework.Repositories;
using Microsoft.EntityFrameworkCore;
using Checklists.Models;

namespace Checklists.Repositories
{
    public class ChecklistTemplateRepository : BaseRepository<ChecklistTemplate>
    {
        public ChecklistTemplateRepository(ChecklistsContext context) : base(context) { }

        protected override DbSet<ChecklistTemplate> _dbset
        {
            get
            {
                return ((ChecklistsContext)_context).ChecklistTemplates;
            }
        }
    }
}