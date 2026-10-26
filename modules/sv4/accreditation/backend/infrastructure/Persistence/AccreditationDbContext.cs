using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Accreditation.Domain.Entities;

namespace ObeAunQa.Modules.Accreditation.Infrastructure;

public partial class AccreditationDbContext : DbContext
{
    public AccreditationDbContext(DbContextOptions<AccreditationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AunCriterion> AunCriteria { get; set; }

    public virtual DbSet<AunCriterionTranslation> AunCriterionTranslations { get; set; }

    public virtual DbSet<AunFramework> AunFrameworks { get; set; }

    public virtual DbSet<AunRequirement> AunRequirements { get; set; }

    public virtual DbSet<AunRequirementTranslation> AunRequirementTranslations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccreditationDbContext).Assembly);
    }

}
