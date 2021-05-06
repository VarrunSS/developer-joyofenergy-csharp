using System;
using System.Collections.Generic;
using System.Linq;
using JOIEnergy.Domain;
using JOIEnergy.Utility;

namespace JOIEnergy.Services
{
    public class PricePlanService : IPricePlanService
    {
        public interface Debug { void Log(string s); };

        private readonly List<PricePlan> _pricePlans;
        private IMeterReadingService _meterReadingService;

        public PricePlanService(List<PricePlan> pricePlan, IMeterReadingService meterReadingService)
        {
            _pricePlans = pricePlan;
            _meterReadingService = meterReadingService;
        }

        private decimal calculateAverageReading(List<ElectricityReading> electricityReadings)
        {
            var newSummedReadings = electricityReadings
                .Select(readings => readings.Reading)
                .Aggregate((reading, accumulator) => reading + accumulator);

            return newSummedReadings / electricityReadings.Count();
        }
        private decimal calculateAverageReadingCost(List<ElectricityReading> electricityReadings, PricePlan pricePlan)
        {
            decimal totalCostPerReading = 0;
            electricityReadings.ForEach(readings =>
                totalCostPerReading += readings.Reading * pricePlan.GetPrice(readings.Time));

            return totalCostPerReading / electricityReadings.Count();
        }
        private decimal calculateTimeElapsed(List<ElectricityReading> electricityReadings)
        {
            var first = electricityReadings.Min(reading => reading.Time);
            var last = electricityReadings.Max(reading => reading.Time);

            return (decimal)(last - first).TotalHours;
        }
        private decimal calculateCost(List<ElectricityReading> electricityReadings, PricePlan pricePlan)
        {
            var average = calculateAverageReadingCost(electricityReadings, pricePlan);
            var timeElapsed = calculateTimeElapsed(electricityReadings);
            var averagedCost = average / timeElapsed;

            return averagedCost;
        }

        public Dictionary<String, decimal> GetConsumptionCostOfElectricityReadingsForEachPricePlan(String smartMeterId)
        {
            List<ElectricityReading> electricityReadings = _meterReadingService.GetReadings(smartMeterId);

            if (!electricityReadings.Any())
            {
                return new Dictionary<string, decimal>();
            }
            return _pricePlans.ToDictionary(
                plan => plan.EnergySupplier.ToString(),
                plan => calculateCost(electricityReadings, plan));
        }

        public Dictionary<string, decimal> GetConsumptionCostOfElectricityReadingsForPricePlan(
            string smartMeterId, Enums.Supplier EnergySupplier, DateTime startDate, DateTime endDate)
        {
            var electricityReadings = _meterReadingService.GetReadings(smartMeterId)
                                            .Where(v => v.Time > startDate && v.Time <= endDate).ToList();

            if (!electricityReadings.Any())
                throw new Exception("No electricity readings found for duration");

            return _pricePlans.Where(v => v.EnergySupplier == EnergySupplier).ToDictionary(
               plan => plan.EnergySupplier.ToString(),
               plan => calculateCost(electricityReadings, plan));
        }
    }
}
