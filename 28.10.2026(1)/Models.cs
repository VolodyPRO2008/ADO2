using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace FootballChampionship
{
    // --- БАЗОВЫЕ ТАБЛИЦЫ ---
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Player> Players { get; set; } = new List<Player>();
    }

    public class Player
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Country { get; set; }
        public int Number { get; set; }
        public string Position { get; set; }
        public int TeamId { get; set; }
        public Team Team { get; set; }
    }

    public class Match
    {
        public int Id { get; set; }
        public DateTime MatchDate { get; set; }
        public int Team1Id { get; set; }
        public Team Team1 { get; set; }
        public int Team2Id { get; set; }
        public Team Team2 { get; set; }
        public int Team1Goals { get; set; }
        public int Team2Goals { get; set; }
        public List<Goal> Goals { get; set; } = new List<Goal>();
    }

    public class Goal
    {
        public int Id { get; set; }
        public int MatchId { get; set; }
        public Match Match { get; set; }
        public int PlayerId { get; set; }
        public Player Player { get; set; }
        public int Minute { get; set; }
    }

    // --- МОДЕЛИ ДЛЯ ПРЕДСТАВЛЕНИЙ (VIEWS) ---
    public class BombardierView
    {
        public int PlayerId { get; set; }
        public string FullName { get; set; }
        public string TeamName { get; set; }
        public int Goals { get; set; }
    }

    public class TeamStatsView
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public int TotalScored { get; set; }
        public int TotalConceded { get; set; }
        public int TotalPoints { get; set; }
    }

    // --- КОНТЕКСТ БАЗЫ ДАННЫХ ---
    public class AppDbContext : DbContext
    {
        public DbSet<Team> Teams { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Goal> Goals { get; set; }

        // DbSet для представлений
        public DbSet<BombardierView> Bombardiers { get; set; }
        public DbSet<TeamStatsView> TeamStats { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // ВНИМАНИЕ: Проверь строку подключения (LocalDB или свой сервер)
            optionsBuilder.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=SpainFootballDB;Trusted_Connection=True;TrustServerCertificate=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Настройки связей матчей
            modelBuilder.Entity<Match>().HasOne(m => m.Team1).WithMany().HasForeignKey(m => m.Team1Id).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Match>().HasOne(m => m.Team2).WithMany().HasForeignKey(m => m.Team2Id).OnDelete(DeleteBehavior.Restrict);

            // Настройки для View (у них нет Primary Key)
            modelBuilder.Entity<BombardierView>().HasNoKey().ToView("View_Bombardiers");
            modelBuilder.Entity<TeamStatsView>().HasNoKey().ToView("View_TeamStats");
        }
    }
}