﻿using Microsoft.EntityFrameworkCore;
using SonCaro.Models;

namespace SonCaro
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Match> Matches { get; set; }
        public DbSet<MatchDetail> MatchDetails { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserRoom> UserRooms { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
    }
}
