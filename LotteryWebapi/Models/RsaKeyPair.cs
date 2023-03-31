namespace LotteryWebApi.Models
{
    public class RsaKeyPair
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
    }
}
