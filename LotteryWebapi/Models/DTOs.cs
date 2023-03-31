namespace LotteryWebApi.Models
{
    public class RegisterRequest
    {
        public string Uid { get; set; }
        public DateTime SoldTime { get; set; }
        public string RetailerId { get; set; }
        public byte[] Numbers { get; set; }
        public byte MegaNumber { get; set; }
        public string ReqSign { get; set; }
    }

    public class RegisterResponse
    {
        public string Uid { get; set; }
        public string RespSign { get; set; }
    }

    public class AddRetailerResponse
    {
        public string Name { get; set; }
        public string PubKey { get; set; }
        public string PrivKey { get; set; }
    }
}
