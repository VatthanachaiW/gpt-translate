﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using DALChatGPT.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DALChatGPT.Contexts;

public interface IChatGPTContext : IDisposable
{
    DatabaseFacade Database { get; }
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
    ChangeTracker ChangeTracker { get; }
    EntityEntry Entry(object entity);
    int SaveChanges();
    Task<int> SaveChangesAsync();
}
public partial class ChatGPTContext : DbContext, IChatGPTContext
{
    public ChatGPTContext(DbContextOptions<ChatGPTContext> options)
        : base(options)
    {
    }

    public virtual DbSet<LanguageType> LanguageTypes { get; set; }

    public virtual DbSet<MailTemplate> MailTemplates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LanguageType>(entity =>
        {
            entity.ToTable("LanguageType");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsDeleted).HasDefaultValueSql("((0))");
            entity.Property(e => e.LanguageName).HasMaxLength(50);
        });

        modelBuilder.Entity<MailTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_MailPhishingTemplate");

            entity.ToTable("MailTemplate");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsDeleted).HasDefaultValueSql("((0))");
            entity.Property(e => e.LanguageId).HasColumnName("LanguageID");
            entity.Property(e => e.MailText).HasMaxLength(500);
            entity.Property(e => e.ParentId).HasColumnName("ParentID");

            entity.HasOne(d => d.Language).WithMany(p => p.MailTemplates)
                .HasForeignKey(d => d.LanguageId)
                .HasConstraintName("FK_MailTemplate_LanguageType");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    public async Task<int> SaveChangesAsync() => await base.SaveChangesAsync();
}