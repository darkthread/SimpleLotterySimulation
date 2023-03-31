using LotteryWebapi;
using LotteryWebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace LotteryWebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;
    private readonly RegDbContext _dbCtx;
    private readonly IRegStore _regStore;

    private readonly LotteryValidator _validator;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, RegDbContext dbCtx, LotteryValidator validator)
    {

        _validator = validator;
        _logger = logger;
        _dbCtx = dbCtx;
        _regStore = _dbCtx as IRegStore;
    }

    // accept a LotteryEntry object and put it to database
    [HttpPost]
    [Route(nameof(Register), Name = nameof(Register))]
    public RegisterResponse Register(RegisterRequest req)
    {
        var respSign = _validator.ValidateAndSign(req);
        var entry = new LotteryEntry(req)
        {
            RespSign = respSign
        };
        _regStore.InsertLotteryEntry(entry);
        return new RegisterResponse
        {
            LotteryUid = req.LotteryUid,
            RespSign = respSign
        };
    }

    [HttpPost]
    [Route(nameof(Validate), Name = nameof(Validate))]
    public bool Validate(Guid lotteryUid, string respSign)
        => _regStore.GetLotterEntry(lotteryUid)?.RespSign == respSign;
    [HttpPost]
    [Route(nameof(AddRetailer), Name = nameof(AddRetailer))]

    public AddRetailerResponse AddRetailer(string name = "00000000-0000-0000-0000-000000000000")
    {
        var crypto = new BCRsaCrypto();
        _regStore.InsertKeyPair(name, crypto.PubKey, crypto.PrivKey);
        return new AddRetailerResponse
        {
            Name = name,
            PubKey = crypto.PubKey,
            PrivKey = crypto.PrivKey
        };
    }
}
