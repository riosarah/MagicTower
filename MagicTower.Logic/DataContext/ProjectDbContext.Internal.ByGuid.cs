//@CodeCopy
#if EXTERNALGUID_ON

using Microsoft.EntityFrameworkCore.ChangeTracking;
using MagicTower.Logic.Entities;

namespace MagicTower.Logic.DataContext
{
    partial class ProjectDbContext
    {
        partial void BeforeSaveChanges(EntityEntry entityEntry)
        {
            if (entityEntry.State == EntityState.Added)
            {
                if (entityEntry.Entity is EntityObject eo && eo.Guid == Guid.Empty)
                {
                    eo.Guid = Guid.NewGuid();
                }
            }
        }
    }
}
#endif
