using Framework.Models;
using Framework.Repositories;
using Microsoft.EntityFrameworkCore;
using Checklists.Models;

namespace Checklists.Repositories
{
    public class ChecklistRepository : BaseRepository<Checklist>
    {
        protected ChecklistTemplateItemRepository TemplateItemRepo { get; private set; }
        protected ChecklistItemRepository ChecklistItemRepo { get; private set; }

        public ChecklistRepository(ChecklistsContext context) : base(context)
        {
            // todo: inject
            TemplateItemRepo = new ChecklistTemplateItemRepository(context);
            ChecklistItemRepo = new ChecklistItemRepository(context);
        }

        public override Checklist CreateItem(Checklist item, string ownerUserName)
        {
            var checklist = base.CreateItem(item, ownerUserName);

            // todo: service! also maybe bulk creation and/or limiting number of items in a template down to <25?
            var templateItems = TemplateItemRepo.GetAllItems(ownerUserName, i => i.ParentId == checklist.ChecklistTemplateId);

            foreach (var templateItem in templateItems)
            {
                var checklistItem = new ChecklistItem() { ParentId = checklist.Id, ChecklistTemplateItemId = templateItem.Id };
                ChecklistItemRepo.CreateItem(checklistItem, ownerUserName);
            }

            return checklist;
        }


        protected override DbSet<Checklist> _dbset
        {
            get
            {
                return ((ChecklistsContext)_context).Checklists;
            }
        }
    }
}