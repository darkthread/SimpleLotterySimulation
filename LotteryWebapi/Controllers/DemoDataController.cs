﻿using System.Text;
using System.Text.Json;
using LotteryWebapi;
using LotteryWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

namespace LotteryWebapi.Controllers
{
    [Route("[controller]")]
    public class DemoDataController : Controller
    {
        private readonly IRegStore _regStore;
        private RegDbContext _dbCtx;
        string retailerDataPath;
        string ticketDataPath;
        public DemoDataController(IWebHostEnvironment env, RegDbContext dbCtx)
        {
            _dbCtx = dbCtx;
            _regStore = dbCtx as IRegStore;
            retailerDataPath = Path.Combine(env.ContentRootPath, "Data", "Retailers");
            ticketDataPath = Path.Combine(env.ContentRootPath, "Data", "Tickets");
        }

        [HttpPost]
        [Route(nameof(GenRetailers))]
        public IActionResult GenRetailers()
        {
            if (_dbCtx.RsaKeyPairs.Count() > 100) return Content("Already exists");
            Directory.CreateDirectory(retailerDataPath);
            for (var i = 0; i < 100; i++)
            {
                var csp = new BCRsaCrypto();
                var kp = new RsaKeyPair
                {
                    Name = i.ToString("0000"),
                    PublicKey = csp.PubKey,
                    PrivateKey = csp.PrivKey
                };
                _regStore.InsertKeyPair(kp.Name, kp.PublicKey, kp.PrivateKey);
                System.IO.File.WriteAllText(Path.Combine(retailerDataPath, kp.Name + ".json"),
                    JsonSerializer.Serialize(kp));
            }

            return Content("OK");
        }

        private static Random rnd = new Random();
        static byte[] GenRandNumbers() => Enumerable.Range(1, 42).OrderBy(i => rnd.Next(i)).Take(6).Select(o => (byte)o).ToArray();
        static byte GetRandMegaNo() => (byte)rnd.Next(1, 8);

        [HttpPost]
        [Route(nameof(GenTestData))]
        public IActionResult GenTestData()
        {
            Directory.CreateDirectory(ticketDataPath);
            var retailers = _dbCtx.RsaKeyPairs.ToList();
            var rnd = new Random();
            for (var i = 0; i < 100; i++)
            {
                var kpName = i.ToString("0000");
                var kp = JsonSerializer.Deserialize<RsaKeyPair>(
                    System.IO.File.ReadAllText(Path.Combine(retailerDataPath, kpName + ".json")));
                var csp = new BCRsaCrypto { PrivKey = kp.PrivateKey };
                var retailerPath = Path.Combine(ticketDataPath, kpName);
                Directory.CreateDirectory(retailerPath);
                for (var j = 0; j < 100; j++)
                {
                    var req = new RegisterRequest
                    {
                        LotteryUid = Guid.NewGuid().ToString(),
                        SoldTime = DateTime.Now,
                        RetailerId = kp.Name,
                        Numbers = GenRandNumbers(),
                        MegaNumber = GetRandMegaNo(),
                    };
                    req.ReqSign = Convert.ToBase64String(csp.Sign(Encoding.UTF8.GetBytes(req.DataString)));
                    System.IO.File.WriteAllText(Path.Combine(retailerPath, $"{j:0000}.json"),
                        JsonSerializer.Serialize(req));
                }
            }

            return Content("OK");

        }

        [HttpPost]
        [Route(nameof(ClearLotteryEntries))]
        public IActionResult ClearLotteryEntries()
        {
            var rowCount = _dbCtx.Database.ExecuteSql($"DELETE FROM LotteryEntries");
            return Content($"{rowCount:n0} rows deleted");
        }

        [HttpPost]
        [Route(nameof(QueryLotteryEntries))]
        public IActionResult QueryLotteryEntries()
        {
            var res = _dbCtx.LotteryEntries.Select(o => new
            {
                o.Id
            }).ToArray();
            return Content($"{res.Count()} rows");
        }
    }
}
