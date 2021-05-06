using System;
using System.Collections.Generic;

namespace JOIEnergy.Services
{
    public interface IPricePlanService
    {
        Dictionary<string, decimal> GetConsumptionCostOfElectricityReadingsForEachPricePlan(string smartMeterId);

        Dictionary<string, decimal> GetConsumptionCostOfElectricityReadingsForPricePlan(
            string smartMeterId, Enums.Supplier EnergySupplier, DateTime startDate, DateTime endDate);

    }
}