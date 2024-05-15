/*using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

class DataEntry
{
    public DateTime TimeFrom { get; set; }
    public DateTime TimeTo { get; set; }
    public double HeatDemand { get; set; }
    public double ElectricityPrice { get; set; }
}

class HeatingOptimizer
{
    public List<DataEntry> WinterData { get; private set; }
    public List<DataEntry> SummerData { get; private set; }

    public void Read()
    {
        WinterData = new List<DataEntry>();
        SummerData = new List<DataEntry>();

        using (var reader = new StreamReader(@"datacsv.csv"))
        {
            reader.ReadLine(); 
            reader.ReadLine(); 

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(';');

                StoreData(values);
            }
        }
    }

    private void StoreData(string[] values)
    {
        var format = "dd/MM/yyyy HH.mm";
        
        var winterEntry = new DataEntry
        {
            TimeFrom = DateTime.ParseExact(values[0], format, CultureInfo.InvariantCulture),
            TimeTo = DateTime.ParseExact(values[1], format, CultureInfo.InvariantCulture),
            HeatDemand = double.Parse(values[2].Replace(',', '.'), CultureInfo.InvariantCulture),
            ElectricityPrice = double.Parse(values[3].Replace(',', '.'), CultureInfo.InvariantCulture)
        };
        WinterData.Add(winterEntry);
        
        if (values.Length > 8)
        {
            var summerEntry = new DataEntry
            {
                TimeFrom = DateTime.ParseExact(values[5], format, CultureInfo.InvariantCulture),
                TimeTo = DateTime.ParseExact(values[6], format, CultureInfo.InvariantCulture),
                HeatDemand = double.Parse(values[7].Replace(',', '.'), CultureInfo.InvariantCulture),
                ElectricityPrice = double.Parse(values[8].Replace(',', '.'), CultureInfo.InvariantCulture)
            };
            SummerData.Add(summerEntry);
        }
    }

    static void Main()
    {
        HeatingOptimizer optimizer = new HeatingOptimizer();
        optimizer.Read();

        double maxCapacityElectric = 8;  
        double maxCapacityGas = 5;       
        double maxCapacityOil = 4;       
        double maxCapacityGasMotor = 3.6;

        double co2Electric = 0;          
        double co2Gas = 215;             
        double co2Oil = 265;             
        double co2GasMotor = 640;        

        
        double costElectric = 50;
        double costGas = 500;
        double costOil = 700;
        double costGasMotor = 1100;

        
        var data = optimizer.WinterData; 
        double totalCost = 0;
        double totalCO2 = 0;
        double maxCO2 = 500; 

        foreach (var entry in data)
        {
            double remainingHeatDemand = entry.HeatDemand;
            double periodCost = 0;
            double periodCO2 = 0;

            
            var boilers = new List<(double maxCapacity, double cost, double co2, string name)>
            {
                (maxCapacityElectric, costElectric + entry.ElectricityPrice, co2Electric, "Electric"),
                (maxCapacityGas, costGas, co2Gas, "Gas"),
                (maxCapacityOil, costOil, co2Oil, "Oil"),
                (maxCapacityGasMotor, costGasMotor, co2GasMotor, "Gas Motor")
            };

            boilers = boilers.OrderBy(b => b.cost).ThenBy(b => b.co2).ToList();

            foreach (var boiler in boilers)
            {
                if (remainingHeatDemand <= 0)
                    break;

                double allocatedHeat = Math.Min(remainingHeatDemand, boiler.maxCapacity);
                double allocatedCost = allocatedHeat * boiler.cost;
                double allocatedCO2 = allocatedHeat * boiler.co2;

                if (periodCO2 + allocatedCO2 <= maxCO2)
                {
                    periodCost += allocatedCost;
                    periodCO2 += allocatedCO2;
                    remainingHeatDemand -= allocatedHeat;

                    Console.WriteLine($"{entry.TimeFrom} to {entry.TimeTo}: Using {boiler.name} Boiler");
                    Console.WriteLine($"Allocated Heat: {allocatedHeat} MW, Cost: {allocatedCost} DKK, CO2: {allocatedCO2} kg");
                }
            }

            if (remainingHeatDemand > 0)
            {
                Console.WriteLine($"Unable to meet full heat demand from {entry.TimeFrom} to {entry.TimeTo} within CO2 constraints.");
            }

            totalCost += periodCost;
            totalCO2 += periodCO2;
        }

        Console.WriteLine($"Total Cost: {totalCost} DKK");
        Console.WriteLine($"Total CO2 Emissions: {totalCO2} kg");
    }
}
*/