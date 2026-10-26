using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Curriculum.Domain.Entities;

namespace ObeAunQa.Modules.Curriculum.Infrastructure;

public partial class CurriculumDbContext : DbContext
{
    public CurriculumDbContext(DbContextOptions<CurriculumDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Clo> Clos { get; set; }

    public virtual DbSet<CloPlo> CloPlos { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<CoursePlo> CoursePlos { get; set; }

    public virtual DbSet<ImportBatch> ImportBatches { get; set; }

    public virtual DbSet<Plo> Plos { get; set; }

    public virtual DbSet<PloBalance> PloBalances { get; set; }

    public virtual DbSet<Program> Programs { get; set; }

    public virtual DbSet<ProgramCourse> ProgramCourses { get; set; }

    public virtual DbSet<ProgramVersion> ProgramVersions { get; set; }

    public virtual DbSet<ProgressionCode> ProgressionCodes { get; set; }

    public virtual DbSet<SourceCourseRow> SourceCourseRows { get; set; }

    public virtual DbSet<SourceDocument> SourceDocuments { get; set; }

    public virtual DbSet<WeightCode> WeightCodes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CurriculumDbContext).Assembly,
            type => type.Namespace == "ObeAunQa.Modules.Curriculum.Infrastructure");
    }

}
