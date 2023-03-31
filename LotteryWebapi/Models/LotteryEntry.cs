using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LotteryWebApi.Models
{
    public class LotteryEntry
    {
        public long Id { get; set; }
        public string LotteryUid { get; set; }
        public DateTime SoldTime { get; set; }
        public string RetailerId { get; set; }
        public byte[] Numbers { get; set; }
        public byte MegaNumber { get; set; }
        public string ReqSign { get; set; }
        public string RespSign { get; set; }
        public string DataString => $"{LotteryUid}\t{SoldTime:yyyyMMddHHmmssfffff}\t{RetailerId}\t{string.Join(",",Numbers.Select(o => o.ToString()).ToArray())}\t{MegaNumber}";
        public LotteryEntry() { }

        public LotteryEntry(RegisterRequest req)
        {
            this.LotteryUid = req.LotteryUid;
            this.SoldTime = req.SoldTime;
            this.RetailerId = req.RetailerId;
            this.Numbers = req.Numbers;
            this.MegaNumber = req.MegaNumber;
            this.ReqSign = req.ReqSign;
        }
    }
}