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
    public class SoundscapeConfigurations : IEntityTypeConfiguration<Soundscape>
    {
        public void Configure(EntityTypeBuilder<Soundscape> builder)
        {
            builder.Property(s => s.Name).HasMaxLength(100).IsRequired();

            builder.HasIndex(s => s.Type);

            builder.HasMany(s => s.Sessions)
                   .WithOne(s => s.Soundscape)
                   .HasForeignKey(s => s.SoundscapeId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
