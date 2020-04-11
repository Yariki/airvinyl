using System.Collections.Generic;
using AirVinyl.Model;
using System.Data.Entity;
using System.Linq;

namespace AirVinyl.DataAccessLayer
{
    public class AirVinylDbContext : DbContext
    {
        public DbSet<Person> People { get; set; }
        public DbSet<VinylRecord> VinylRecords { get; set; }
        public DbSet<RecordStore> RecordStores { get; set; }
        public DbSet<PressingDetail> PressingDetails { get; set; }

        public DbSet<DynamicProperty> DynamicProperties { get; set; }
    
        public AirVinylDbContext()
        {
            Database.SetInitializer(new AirVinylDBInitializer());
            // disable lazy loading
            Configuration.LazyLoadingEnabled = false;          
        }

        public override int SaveChanges()
        {
	        var modifiedOrAddedVinylRecords = ChangeTracker.Entries<VinylRecord>()
	                                                       .Where(e =>
		                                                       e.State == EntityState.Added ||
		                                                       e.State == EntityState.Modified ||
		                                                       e.State == EntityState.Unchanged).ToList();
	        for (int i = 0; i < modifiedOrAddedVinylRecords.Count; i++)
	        {
		        var vinylRecord = modifiedOrAddedVinylRecords[i];

                var dynamicProperties = new List<DynamicProperty>();
		        foreach (var entityProperty in vinylRecord.Entity.Properties)
		        {
			        dynamicProperties.Add(new DynamicProperty()
			        {
                        Key = entityProperty.Key,
                        Value = entityProperty.Value
			        });
		        }
                vinylRecord.Entity.DynamicProperties.Clear();

                foreach (var dynamicProperty in dynamicProperties)
                {
	                var existing = ChangeTracker.Entries<DynamicProperty>()
	                                            .FirstOrDefault(d => d.Entity.Key == dynamicProperty.Key);
                    if(existing != null)
                    {
	                    DynamicProperties.Remove(existing.Entity);
                    }
                    vinylRecord.Entity.DynamicProperties.Add(dynamicProperty);
                }
	        }

	        return base.SaveChanges();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // ensure the same person can be added to different collections
            // of friends (self-referencing many-to-many relationship)
            modelBuilder.Entity<Person>().HasMany(m => m.Friends).WithMany();

            modelBuilder.Entity<Person>().HasMany(p => p.VinylRecords)
                .WithRequired(r => r.Person).WillCascadeOnDelete(true);
        } 
    }
}
