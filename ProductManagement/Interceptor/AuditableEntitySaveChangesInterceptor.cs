using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;
using ProductManagement.Entity;

namespace ProductManagement.Interceptor
{
    public class AuditableEntitySaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditableEntitySaveChangesInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            TrackChanges(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            TrackChanges(eventData.Context);
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void TrackChanges(DbContext context)
        {
            if (context == null) return;

            var auditEntries = new List<ProductAudit>();
            var userId = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";

            foreach (var entry in context.ChangeTracker.Entries<Product>())
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                {
                    var auditEntry = new ProductAudit
                    {
                        ProductId = entry.Entity.Id,
                        ChangeType = entry.State.ToString(),
                        ChangedBy = userId,
                        ChangeDate = DateTime.UtcNow,
                        OldValue = entry.State == EntityState.Added ? null : JsonConvert.SerializeObject(entry.OriginalValues.Properties.ToDictionary(p => p.Name, p => entry.OriginalValues[p.Name])),
                        NewValue = entry.State == EntityState.Deleted ? null : JsonConvert.SerializeObject(entry.CurrentValues.Properties.ToDictionary(p => p.Name, p => entry.CurrentValues[p.Name]))
                    };

                    auditEntries.Add(auditEntry);
                }
            }

            context.Set<ProductAudit>().AddRange(auditEntries);
        }
    }

}
