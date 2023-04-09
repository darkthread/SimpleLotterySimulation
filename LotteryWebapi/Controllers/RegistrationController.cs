using LotteryWebapi;
using LotteryWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Ocsp;

namespace LotteryWebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class RegistrationController : ControllerBase
{
    private readonly ILogger<RegistrationController> _logger;
    private readonly RegDbContext _dbCtx;
    private readonly IRegStore _regStore;

    private readonly LotteryValidator _validator;

    public RegistrationController(ILogger<RegistrationController> logger, RegDbContext dbCtx, LotteryValidator validator)
    {

        _validator = validator;
        _logger = logger;
        _dbCtx = dbCtx;
        _regStore = _dbCtx as IRegStore;
    }

    // accept a LotteryEntry object and put it to database
    [HttpPost]
    [Route(nameof(Register))]
    public IActionResult Register(RegisterRequest req)
    {
        try
        {
            var respSign = _validator.ValidateAndSign(req);
            var entry = new LotteryEntry(req)
            {
                RespSign = respSign
            };
            _regStore.InsertLotteryEntry(entry);
            return Ok(new RegisterResponse
            {
                LotteryUid = req.LotteryUid,
                RespSign = respSign
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = ex.Message
            });
        }
    }

    [HttpPost]
    [Route(nameof(TestJson))]
    public IActionResult TestJson(RegisterRequest req)
    {
        try
        {
            return Ok(new RegisterResponse
            {
                LotteryUid = req.LotteryUid,
                RespSign = "OK"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = ex.ToString()
            });
        }
    }

    [HttpPost]
    [Route(nameof(TestForm))]
    [Consumes("application/x-www-form-urlencoded")]
    public IActionResult TestForm(
        [FromForm] string lotteryUid,
        [FromForm] DateTime soldTime,
        [FromForm] string retailerId, 
        [FromForm] string numbers, 
        [FromForm] byte megaNumber,
        [FromForm] string reqSign
    )
    {

        try
        {
            var req = new RegisterRequest
            {
                LotteryUid = lotteryUid,
                RetailerId = retailerId,
                ReqSign= reqSign,
                Numbers = numbers.Split(',').Select(o => byte.Parse(o)).ToArray(),
                MegaNumber = megaNumber,
                SoldTime = soldTime
            };
            return Ok($"{req.LotteryUid}\tOK");
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = ex.ToString()
            });
        }
    }


    [HttpPost]
    [Route(nameof(Validate))]
    public bool Validate(Guid lotteryUid, string respSign)
        => _regStore.GetLotterEntry(lotteryUid)?.RespSign == respSign;
    [HttpPost]
    [Route(nameof(AddRetailer))]

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
    [HttpPost]
    [Route(nameof(GetNow))]
    public string GetNow()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
    }
}
