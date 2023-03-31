namespace LotteryWebApi.Models
{
    public interface IRegStore
    {
        string GetPublicKey(string name);
        string GetPrivateKey(string name);
        void InsertLotteryEntry(LotteryEntry entry);
        LotteryEntry GetLotterEntry(Guid lotteryUid);

        void InsertKeyPair(string name, string pubKey, string privKey);

    }
}
