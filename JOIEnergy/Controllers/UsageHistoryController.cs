using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JOIEnergy.Enums;
using JOIEnergy.Services;
using JOIEnergy.Utility;
using Microsoft.AspNetCore.Mvc;

namespace JOIEnergy.Controllers
{
    [Route("usage-history")]
    public class UsageHistoryController : Controller
    {
        private readonly IMeterReadingService _meterReadingService;
        private readonly IAccountService _accountService;
        private readonly IPricePlanService _pricePlanService;

        public UsageHistoryController(IMeterReadingService meterReadingService,
            IAccountService accountService, IPricePlanService pricePlanService)
        {
            _meterReadingService = meterReadingService;
            _accountService = accountService;
            _pricePlanService = pricePlanService;
        }

        [HttpGet("last-week/{smart-meter-id}")]
        public ObjectResult GetLastWeekUsage(string smartMeterId)
        {
            var pricePlanId = _accountService.GetPricePlanIdForSmartMeterId(smartMeterId);

            (bool isValid, string message) = IsValidLastWeekUsage(smartMeterId, pricePlanId);
            if (!isValid)
                return new NotFoundObjectResult(message);

            int interval = 7;
            DateTime endDate = DateTime.Now.StartOfWeek(DayOfWeek.Sunday);
            DateTime startDate = endDate.AddDays(-interval);
            
            var result = _pricePlanService.GetConsumptionCostOfElectricityReadingsForPricePlan(smartMeterId, pricePlanId, startDate, endDate);

            if (!result.Any()) return new NotFoundObjectResult("{}");
            return new OkObjectResult(result);
        }

        private (bool, string) IsValidLastWeekUsage(string smartMeterId, Enums.Supplier pricePlanId)
        {
            var meterReadings = _meterReadingService.GetReadings(smartMeterId);

            if (meterReadings == null && !meterReadings.Any()) return (false, "Invalid Smart Meter ID");
            if (pricePlanId == Enums.Supplier.NullSupplier) return (false, "Plan not mapped to Smart Meter ID");

            return (true, string.Empty);
        }
    }
}