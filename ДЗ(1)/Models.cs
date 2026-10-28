using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace FootballChampionship
{
    // --- СУЩНОСТИ (Таблицы) ---

    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Player> Players { get; set; } = new List<Player>();
    }

    public class Player
    {
        public int Id { get; set; }
        public string FullName { get; set; } // ФИО
        public string Country { get; set; }  // Страна
        public int Number { get; set; }      // Номер
        public string Position { get; set; } // Позиция

        public int TeamId { get; set; }
        public Team Team { get; set; }
    }

    public class Match
    {
        public int Id { get; set; }
        public DateTime MatchDate { get; set; } // Дата матча

        public int Team1Id { get; set; }
        public Team Team1 { get; set; }

        public int Team2Id { get; set; }
        public Team Team2 { get; set; }

        public int Team1Goals { get; set; } // Забито командой 1
        public int Team2Goals { get; set; } // Забито командой 2

        public List<Goal> Goals { get; set; } = new List<Goal>();
    }

    public class Goal
    {
        public int Id { get; set; }

        public int MatchId { get; set; }
        public Match Match { get; set; }

        public int PlayerId { get; set; }
        public Player Player { get; set; } // Кто забил

        public int Minute { get; set; } // На какой минуте
    }

    //  КОНТЕКСТ БАЗЫ ДАННЫХ 

    public class AppDbContext : DbContext
    {
        public DbSet<Team> Teams { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Goal> Goals { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=SpainFootballDB;Trusted_Connection=True;TrustServerCertificate=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Защита от каскадного удаления, чтобы EF понимал разницу между Team1 и Team2
            modelBuilder.Entity<Match>()
                .HasOne(m => m.Team1).WithMany().HasForeignKey(m => m.Team1Id).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.Team2).WithMany().HasForeignKey(m => m.Team2Id).OnDelete(DeleteBehavior.Restrict);
        }
    }
}