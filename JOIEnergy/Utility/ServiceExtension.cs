using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JOIEnergy.Services;
using JOIEnergy.Domain;
using JOIEnergy.Enums;
using JOIEnergy.Generator;

namespace JOIEnergy.Utility
{
    public static class ServiceExtension
    {

        public static void Inject(this IServiceCollection services)
        {
            services.AddTransient<IAccountService, AccountService>();
            services.AddTransient<IMeterReadingService, MeterReadingService>();
            services.AddTransient<IPricePlanService, PricePlanService>();
            SetMockData(services);
        }

        public static void SetMockData(this IServiceCollection services)
        {
            services.AddSingleton((IServiceProvider arg) => GenerateMeterElectricityReadings());
            services.AddSingleton((IServiceProvider arg) => GetPricePlans());
            services.AddSingleton((IServiceProvider arg) => SmartMeterToPricePlanAccounts);
        }


        private static List<PricePlan> GetPricePlans()
        {
            return new List<PricePlan> {
                new PricePlan{
                    EnergySupplier = Enums.Supplier.DrEvilsDarkEnergy,
                    UnitRate = 10m,
                    PeakTimeMultiplier = new List<PeakTimeMultiplier>()
                    {
                        new PeakTimeMultiplier() { DayOfWeek = DayOfWeek.Tuesday, Multiplier = 1 }
                    }
                },
                new PricePlan{
                    EnergySupplier = Enums.Supplier.TheGreenEco,
                    UnitRate = 2m,
                    PeakTimeMultiplier = new List<PeakTimeMultiplier>()
                     {
                        new PeakTimeMultiplier() { DayOfWeek = DayOfWeek.Tuesday, Multiplier = 15 }
                    }
                },
                new PricePlan{
                    EnergySupplier = Enums.Supplier.PowerForEveryone,
                    UnitRate = 1m,
                    PeakTimeMultiplier = new List<PeakTimeMultiplier>()
                     {
                        new PeakTimeMultiplier() { DayOfWeek = DayOfWeek.Tuesday, Multiplier = 8 }
                    }
                }
            };
            ;
        }

        private static Dictionary<string, List<ElectricityReading>> GenerateMeterElectricityReadings()
        {
            var readings = new Dictionary<string, List<ElectricityReading>>();
            var generator = new ElectricityReadingGenerator();
            var smartMeterIds = SmartMeterToPricePlanAccounts.Select(mtpp => mtpp.Key);

            foreach (var smartMeterId in smartMeterIds)
            {
                readings.Add(smartMeterId, generator.Generate(20));
            }
            return readings;
        }

        public static Dictionary<string, Supplier> SmartMeterToPricePlanAccounts
        {
            get
            {
                return new Dictionary<string, Supplier>
                {
                    { "smart-meter-0", Supplier.DrEvilsDarkEnergy },
                    { "smart-meter-1", Supplier.TheGreenEco },
                    { "smart-meter-2", Supplier.DrEvilsDarkEnergy },
                    { "smart-meter-3", Supplier.PowerForEveryone },
                    { "smart-meter-4", Supplier.TheGreenEco }
                };
            }
        }

    }
}
