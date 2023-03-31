using System.Text.Json;
using LotteryWebapi;
using LotteryWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers;

namespace lottery_webapi.Controllers
{
    public class DemoDataController : Controller
    {
        private readonly IRegStore _regStore;
        string retailerDataPath;
        string ticketDataPath;
        public DemoDataController(IWebHostEnvironment env, RegDbContext dbCtx)
        {
            _regStore = dbCtx as IRegStore;
            retailerDataPath = Path.Combine(env.ContentRootPath, "Data", "Retailers");
             ticketDataPath = Path.Combine(env.ContentRootPath, "Data", "Tickets");
        }
        public IActionResult Index()
        {
            return Content("Demo Data Creator");
        }

        public IActionResult RegRetailers()
        {
            for (var i = 0; i < 100; i++)
            {
                var csp = new BCRsaCrypto();
                var kp = new RsaKeyPair
                {
                    Name= i.ToString("0000"),
                    PublicKey = csp.PubKey,
                    PrivateKey = csp.PrivKey
                };
                _regStore.InsertKeyPair(kp.Name, kp.PublicKey, kp.PrivateKey);
                System.IO.File.WriteAllText(Path.Combine(retailerDataPath, kp.Name + ".json"),
                    JsonSerializer.Serialize(kp));
            }

            return Content("OK");
        }
    }
}
