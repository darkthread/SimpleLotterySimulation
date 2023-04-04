using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using LotteryWebapi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace LotteryWebApi.Models
{
    public class RegDbContext : DbContext, IRegStore
    {
        private readonly IMemoryCache _memroyCache;
        public DbSet<LotteryEntry> LotteryEntries { get; set; }
        public DbSet<RsaKeyPair> RsaKeyPairs { get; set; }
        public RegDbContext(DbContextOptions<RegDbContext> options, IMemoryCache memroyCache) : base(options)
        {
            _memroyCache = memroyCache;
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

        RsaKeyPair GetKeyPair(string name)
        {
            if (_memroyCache.TryGetValue(name, out var key)) { return key as RsaKeyPair; }

            lock (_memroyCache)
            {
                if (_memroyCache.TryGetValue(name, out key)) { return key as RsaKeyPair; }
                var keyPair = RsaKeyPairs.SingleOrDefault(o => o.Name == name);
                if (keyPair == null) { throw new ApplicationException($"Could not find key pair for {name}"); }
                _memroyCache.Set(name, keyPair);
                return keyPair;
            }
        }

        public string GetPublicKey(string name) => GetKeyPair(name)?.PublicKey
                                                   ?? throw new ApplicationException($"Could not find public key for {name}");
        public string GetPrivateKey(string name) => GetKeyPair(name)?.PrivateKey 
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