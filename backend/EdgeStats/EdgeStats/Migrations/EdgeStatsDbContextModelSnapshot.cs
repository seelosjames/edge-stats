﻿// <auto-generated />
using System;
using EdgeStats;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EdgeStats.Migrations
{
    [DbContext(typeof(EdgeStatsDbContext))]
    partial class EdgeStatsDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("EdgeStats.models.Game", b =>
                {
                    b.Property<int>("GameId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("GameId"));

                    b.Property<DateTime>("GameDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("GameUuid")
                        .HasColumnType("uuid");

                    b.Property<int>("LeagueId")
                        .HasColumnType("integer");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<int>("Team1Id")
                        .HasColumnType("integer");

                    b.Property<int>("Team2Id")
                        .HasColumnType("integer");

                    b.HasKey("GameId");

                    b.HasIndex("LeagueId");

                    b.HasIndex("Team1Id");

                    b.HasIndex("Team2Id");

                    b.ToTable("Games");
                });

            modelBuilder.Entity("EdgeStats.models.League", b =>
                {
                    b.Property<int>("LeagueId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("LeagueId"));

                    b.Property<string>("LeagueName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("SportType")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.HasKey("LeagueId");

                    b.ToTable("Leagues");
                });

            modelBuilder.Entity("EdgeStats.models.Line", b =>
                {
                    b.Property<int>("LineId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("LineId"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<Guid>("LineUuid")
                        .HasColumnType("uuid");

                    b.Property<decimal>("Odd")
                        .HasColumnType("decimal(8,4)")
                        .HasColumnName("odd");

                    b.Property<int>("PropId")
                        .HasColumnType("integer");

                    b.Property<int>("SportsbookId")
                        .HasColumnType("integer");

                    b.HasKey("LineId");

                    b.HasIndex("PropId");

                    b.HasIndex("SportsbookId");

                    b.ToTable("Lines");
                });

            modelBuilder.Entity("EdgeStats.models.Prop", b =>
                {
                    b.Property<int>("PropId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("PropId"));

                    b.Property<int>("GameId")
                        .HasColumnType("integer");

                    b.Property<string>("PropName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("PropType")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<Guid>("PropUuid")
                        .HasColumnType("uuid");

                    b.HasKey("PropId");

                    b.HasIndex("GameId");

                    b.ToTable("Props");
                });

            modelBuilder.Entity("EdgeStats.models.Sportsbook", b =>
                {
                    b.Property<int>("SportsbookId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("SportsbookId"));

                    b.Property<string>("SportsbookName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.HasKey("SportsbookId");

                    b.ToTable("Sportsbooks");
                });

            modelBuilder.Entity("EdgeStats.models.Team", b =>
                {
                    b.Property<int>("TeamId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("TeamId"));

                    b.Property<int>("LeagueId")
                        .HasColumnType("integer");

                    b.Property<string>("TeamName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.HasKey("TeamId");

                    b.HasIndex("LeagueId");

                    b.ToTable("Teams");
                });

            modelBuilder.Entity("EdgeStats.models.WatchlistItem", b =>
                {
                    b.Property<int>("WatchListItemsId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("WatchListItemsId"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("LineId")
                        .HasColumnType("integer");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("WatchListItemsId");

                    b.HasIndex("LineId");

                    b.ToTable("WatchlistItems");
                });

            modelBuilder.Entity("EdgeStats.models.Game", b =>
                {
                    b.HasOne("EdgeStats.models.League", "League")
                        .WithMany("Games")
                        .HasForeignKey("LeagueId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("EdgeStats.models.Team", "Team1")
                        .WithMany("GamesAsTeam1")
                        .HasForeignKey("Team1Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("EdgeStats.models.Team", "Team2")
                        .WithMany("GamesAsTeam2")
                        .HasForeignKey("Team2Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("League");

                    b.Navigation("Team1");

                    b.Navigation("Team2");
                });

            modelBuilder.Entity("EdgeStats.models.Line", b =>
                {
                    b.HasOne("EdgeStats.models.Prop", "Prop")
                        .WithMany()
                        .HasForeignKey("PropId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("EdgeStats.models.Sportsbook", "Sportsbook")
                        .WithMany()
                        .HasForeignKey("SportsbookId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Prop");

                    b.Navigation("Sportsbook");
                });

            modelBuilder.Entity("EdgeStats.models.Prop", b =>
                {
                    b.HasOne("EdgeStats.models.Game", "Game")
                        .WithMany()
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Game");
                });

            modelBuilder.Entity("EdgeStats.models.Team", b =>
                {
                    b.HasOne("EdgeStats.models.League", "League")
                        .WithMany("Teams")
                        .HasForeignKey("LeagueId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("League");
                });

            modelBuilder.Entity("EdgeStats.models.WatchlistItem", b =>
                {
                    b.HasOne("EdgeStats.models.Line", "Line")
                        .WithMany()
                        .HasForeignKey("LineId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Line");
                });

            modelBuilder.Entity("EdgeStats.models.League", b =>
                {
                    b.Navigation("Games");

                    b.Navigation("Teams");
                });

            modelBuilder.Entity("EdgeStats.models.Team", b =>
                {
                    b.Navigation("GamesAsTeam1");

                    b.Navigation("GamesAsTeam2");
                });
#pragma warning restore 612, 618
        }
    }
}
