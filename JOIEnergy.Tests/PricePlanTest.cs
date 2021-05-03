using System;
using System.Collections.Generic;
using JOIEnergy.Domain;
using JOIEnergy.Enums;
using Xunit;

namespace JOIEnergy.Tests
{
    public class PricePlanTest
    {
        private PricePlan _pricePlan;

        public PricePlanTest()
        {
            _pricePlan = new PricePlan
            {
                EnergySupplier = Supplier.TheGreenEco,
                UnitRate = 20m,
                PeakTimeMultiplier = new List<PeakTimeMultiplier> {
                    new PeakTimeMultiplier {
                        DayOfWeek = DayOfWeek.Saturday,
                        Multiplier = 2m
                    },
                    new PeakTimeMultiplier {
                        DayOfWeek = DayOfWeek.Sunday,
                        Multiplier = 10m
                    }
                }
            };
        }

        [Fact]
        public void TestGetEnergySupplier()
        {
            Assert.Equal(Supplier.TheGreenEco, _pricePlan.EnergySupplier);
        }

        [Fact]
        public void TestGetBasePrice()
        {
            Assert.Equal(20m, _pricePlan.GetPrice(new DateTime(2018, 1, 2)));
        }

        [Theory]
        [MemberData(nameof(TestGetPeakTimePriceInput))]
        public void TestGetPeakTimePrice(decimal expected, DateTime date)
        {
            Assert.Equal(expected, _pricePlan.GetPrice(date));
        }

        public static readonly object[][] TestGetPeakTimePriceInput =
        {
            new object[] { 40m, new DateTime(2018, 1, 6) },
            new object[] { 200m, new DateTime(2018, 1, 7) }
        };

    }
}
