using System.Text;
using LotteryWebapi;

namespace LotteryWebApi.Models
{
    public class LotteryValidator
    {
        public const string MasterKeyName = "master";
        private readonly RegDbContext _dbCtx;
        private readonly BCRsaCrypto _masterCrypto;
        public LotteryValidator(RegDbContext dbCtx)
        {
            _dbCtx = dbCtx;
            _masterCrypto = new BCRsaCrypto()
            {
                PrivKey = _dbCtx.GetPrivateKey(MasterKeyName)
            };
        }

        public string ValidateAndSign(RegisterRequest req)
        {
            var pubKey = _dbCtx.GetPublicKey(req.RetailerId);
            var entry = new LotteryEntry(req);
            var data = Encoding.UTF8.GetBytes(entry.DataString);
            var crypto = new BCRsaCrypto()
            {
                PubKey = pubKey
            };
            if (!crypto.Verify(data, Convert.FromBase64String(entry.ReqSign)))
                throw new ApplicationException("Invalid request signature");
            return Convert.ToBase64String(_masterCrypto.Sign(data));
        }
    }
}
