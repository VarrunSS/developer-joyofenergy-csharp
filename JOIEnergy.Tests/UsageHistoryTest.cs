using JOIEnergy.Controllers;
using JOIEnergy.Domain;
using JOIEnergy.Enums;
using JOIEnergy.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace JOIEnergy.Tests
{
    public class UsageHistoryTest
    {
        private readonly MeterReadingService meterReadingService;
        private readonly UsageHistoryController controller;
        private readonly Dictionary<string, Supplier> smartMeterToPricePlanAccounts = new Dictionary<string, Supplier>();
        private static String SMART_METER_ID = "smart-meter-id";

        public UsageHistoryTest()
        {
            smartMeterToPricePlanAccounts.Add(SMART_METER_ID, Supplier.DrEvilsDarkEnergy);

            var readings = new Dictionary<string, List<ElectricityReading>>() {
                { SMART_METER_ID, new List<ElectricityReading>() {
                    new ElectricityReading() { Reading = 1m, Time = new DateTime(2021, 05, 01) },
                    new ElectricityReading() { Reading = 2m, Time = new DateTime(2021, 04, 30) },
                    new ElectricityReading() { Reading = 3m, Time = new DateTime(2021, 04, 29) },
                    new ElectricityReading() { Reading = 4m, Time = new DateTime(2021, 04, 28) },
                    new ElectricityReading() { Reading = 5m, Time = new DateTime(2021, 04, 27) },
                }}};
            meterReadingService = new MeterReadingService(readings);
            var pricePlans = new List<PricePlan>() {
                new PricePlan() { EnergySupplier = Supplier.DrEvilsDarkEnergy, UnitRate = 10, PeakTimeMultiplier = NoMultipliers() },
                new PricePlan() { EnergySupplier = Supplier.TheGreenEco, UnitRate = 2, PeakTimeMultiplier = NoMultipliers() },
                new PricePlan() { EnergySupplier = Supplier.PowerForEveryone, UnitRate = 1, PeakTimeMultiplier = NoMultipliers() }
            };
            var pricePlanService = new PricePlanService(pricePlans, meterReadingService);
            var accountService = new AccountService(smartMeterToPricePlanAccounts);
            controller = new UsageHistoryController(meterReadingService, accountService, pricePlanService);

        }

        [Fact]
        public void TestGetLastWeekUsage()
        {
            // Arrange
            var dateDiff = (decimal)(new DateTime(2021, 05, 01) - new DateTime(2021, 04, 27)).TotalHours;
            var expected = 3m * 10 / dateDiff;

            // Act
            object result = controller.GetLastWeekUsage(SMART_METER_ID).Value;
            var actual = ((IEnumerable<KeyValuePair<string, decimal>>)result).ToList();

            // Assert
            Assert.Equal(1, actual.Count);
            Assert.Equal(expected, actual.First().Value);
        }

        [Fact]
        public void TestGetLastWeekUsageExceptions()
        {
            Assert.Equal(404, controller.GetLastWeekUsage("not-found").StatusCode); // invalid smart meter
            Assert.Equal(404, controller.GetLastWeekUsage("smart-meter-5").StatusCode); // not mapped
            Assert.Equal(404, controller.GetLastWeekUsage("smart-meter-1").StatusCode); // does not have any last week
        }


        private static List<PeakTimeMultiplier> NoMultipliers()
        {
            return new List<PeakTimeMultiplier>();
        }
    }
}
