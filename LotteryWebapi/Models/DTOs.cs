using System.Text.Json.Serialization;

namespace LotteryWebApi.Models
{
    public class RegisterRequest
    {
        public string LotteryUid { get; set; }
        public DateTime SoldTime { get; set; }
        public string RetailerId { get; set; }
        public byte[] Numbers { get; set; }
        public byte MegaNumber { get; set; }
        public string ReqSign { get; set; }
        [JsonIgnore]
        public string DataString => $"{LotteryUid}\t{SoldTime:yyyyMMddHHmmssfffff}\t{RetailerId}\t{string.Join(",", Numbers.Select(o => o.ToString()).ToArray())}\t{MegaNumber}";
    }

    public class RegisterResponse
    {
        public string LotteryUid { get; set; }
        public string RespSign { get; set; }
    }

    public class AddRetailerResponse
    {
        public string Name { get; set; }
        public string PubKey { get; set; }
        public string PrivKey { get; set; }
    }
}
