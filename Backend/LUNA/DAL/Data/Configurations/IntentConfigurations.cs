using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations
{
    public class IntentConfigurations : IEntityTypeConfiguration<Intent>
    {
        public void Configure(EntityTypeBuilder<Intent> builder)
        {
            builder.Property(i => i.Name).HasMaxLength(100).IsRequired();

            builder.HasMany(i => i.Sessions)
                   .WithOne(s => s.Intent)
                   .HasForeignKey(s => s.IntentId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
