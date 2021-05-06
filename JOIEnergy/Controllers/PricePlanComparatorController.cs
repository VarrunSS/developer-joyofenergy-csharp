using System.Collections.Generic;
using System.Linq;
using JOIEnergy.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace JOIEnergy.Controllers
{
    [Route("price-plans")]
    public class PricePlanComparatorController : Controller
    {
        private readonly IPricePlanService _pricePlanService;
        private readonly IAccountService _accountService;

        public PricePlanComparatorController(IPricePlanService pricePlanService,
            IAccountService accountService)
        {
            this._pricePlanService = pricePlanService;
            this._accountService = accountService;
        }

        [HttpGet("compare-all/{smartMeterId}")]
        public ObjectResult CalculatedCostForEachPricePlan(string smartMeterId)
        {
            var pricePlanId = _accountService.GetPricePlanIdForSmartMeterId(smartMeterId);
            var costPerPricePlan = _pricePlanService.GetConsumptionCostOfElectricityReadingsForEachPricePlan(smartMeterId);
            if (!costPerPricePlan.Any())
            {
                return new NotFoundObjectResult(string.Format("Smart Meter ID ({0}) not found", smartMeterId));
            }

            dynamic response = JObject.FromObject(costPerPricePlan);

            return
                costPerPricePlan.Any() ?
                new ObjectResult(response) :
                new NotFoundObjectResult(string.Format("Smart Meter ID ({0}) not found", smartMeterId));
        }

        [HttpGet("recommend/{smartMeterId}")]
        public ObjectResult RecommendCheapestPricePlans(string smartMeterId, int? limit = null)
        {
            var consumptionForPricePlans = _pricePlanService.GetConsumptionCostOfElectricityReadingsForEachPricePlan(smartMeterId);

            if (!consumptionForPricePlans.Any())
            {
                return new NotFoundObjectResult(string.Format("Smart Meter ID ({0}) not found", smartMeterId));
            }

            var recommendations = consumptionForPricePlans.OrderBy(pricePlanComparison => pricePlanComparison.Value);

            if (limit.HasValue && limit.Value < recommendations.Count())
            {
                return new ObjectResult(recommendations.Take(limit.Value));
            }

            return new ObjectResult(recommendations);
        }

        //[HttpGet("last-week-usage/{smartMeterId}")]
        //public ObjectResult CalculateLastWeekUsageForPricePlan(string smartMeterId)
        //{
        //    var pricePlanId = _accountService.GetPricePlanIdForSmartMeterId(smartMeterId);
        //    var costForLastWeek = _pricePlanService.CalculateLastWeekUsageForPricePlan(smartMeterId, pricePlanId);
        //    var (isValid, message) = IsMeterReadingsValid(smartMeterId, pricePlanId, costForLastWeek);
        //    if (!isValid)
        //    {
        //        return new NotFoundObjectResult(message);
        //    }

        //    dynamic response = JObject.FromObject(costForLastWeek);
        //    return new ObjectResult(response);
        //}

        //private (bool isValid, string message) IsMeterReadingsValid(string smartMeterId, Enums.Supplier pricePlanId, Dictionary<string, decimal> costForLastWeek)
        //{
        //    var meterReadings = _meterReadingService.GetReadings(smartMeterId);
        //    if (smartMeterId == null || !smartMeterId.Any()) return (false, string.Format("Smart Meter ID({0}) not found", smartMeterId));
        //    if (meterReadings == null || !meterReadings.Any()) return (false, string.Format("Meter reading not found", smartMeterId));
        //    if (costForLastWeek == null || !costForLastWeek.Any()) return (false, string.Format("Last week reading not found", smartMeterId));
        //    return (true, "");
        //}
    }
}
