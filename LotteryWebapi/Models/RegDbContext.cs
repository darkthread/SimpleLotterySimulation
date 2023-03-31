using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using LotteryWebapi;
using Microsoft.EntityFrameworkCore;

namespace LotteryWebApi.Models
{
    public class RegDbContext : DbContext, IRegStore
    {
        public DbSet<LotteryEntry> LotteryEntries { get; set; }
        public DbSet<RsaKeyPair> RsaKeyPairs { get; set; }
        public RegDbContext(DbContextOptions<RegDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LotteryEntry>().HasKey(o => o.LotteryUid);
            modelBuilder.Entity<RsaKeyPair>().HasKey(o => o.Name);
        }

        public void Init()
        {
            this.Database.EnsureCreated();
            if (!this.RsaKeyPairs.Any())
            {
                var crypto = new BCRsaCrypto();
                this.RsaKeyPairs.Add(new RsaKeyPair
                {
                    Name = LotteryValidator.MasterKeyName,
                    PublicKey = crypto.PubKey,
                    PrivateKey = crypto.PrivKey
                });
                this.SaveChanges();
            }
        }

        public string GetPublicKey(string name) => RsaKeyPairs.SingleOrDefault(o => o.Name == name)?.PublicKey
                                                   ?? throw new ApplicationException($"Could not find public key for {name}");
        public string GetPrivateKey(string name) => RsaKeyPairs.SingleOrDefault(o => o.Name == name)?.PrivateKey
                                                    ?? throw new ApplicationException($"Could not find private key for {name}");

        public void InsertLotteryEntry(LotteryEntry entry)
        {
            LotteryEntries.Add(entry);
            this.SaveChanges();
        }

        public LotteryEntry GetLotterEntry(Guid lotteryUid)
            => LotteryEntries.SingleOrDefault(o => o.LotteryUid == lotteryUid.ToString());

        public void InsertKeyPair(string name, string pubKey, string privKey)
        {
            RsaKeyPairs.Add(new RsaKeyPair { Name = name, PublicKey = pubKey, PrivateKey = privKey });
            this.SaveChanges();
        }

    }
}