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

        double maxCapacityElectric = 200;
        double maxCapacityDiesel = 150;
        double maxCapacityGas = 180;

        double co2Electric = 0.5;
        double co2Diesel = 2.7;
        double co2Gas = 1.9;
        double maxCO2 = 500;

        double dieselPrice = 60;
        double gasPrice = 45;

        var data = optimizer.WinterData;
        double[] Q_electric = new double[data.Count];
        double[] Q_diesel = new double[data.Count];
        double[] Q_gas = new double[data.Count];
        bool[] On_electric = new bool[data.Count];
        bool[] On_diesel = new bool[data.Count];
        bool[] On_gas = new bool[data.Count];

        for (int t = 0; t < data.Count; t++)
        {
            Q_electric[t] = 0;
            Q_diesel[t] = 0;
            Q_gas[t] = 0;
            On_electric[t] = false;
            On_diesel[t] = false;
            On_gas[t] = false;
        }

        double totalCost = 0;
        double totalCO2 = 0;

        for (int t = 0; t < data.Count; t++)
        {
            double consumption = data[t].HeatDemand;
            double remainingConsumption = consumption;

            if (remainingConsumption > 0 && totalCO2 + (remainingConsumption * co2Electric) <= maxCO2)
            {
                double supply = Math.Min(remainingConsumption, maxCapacityElectric);
                Q_electric[t] = supply;
                On_electric[t] = supply > 0;
                remainingConsumption -= supply;
                totalCost += supply * data[t].ElectricityPrice;
                totalCO2 += supply * co2Electric;
            }

            if (remainingConsumption > 0 && totalCO2 + (remainingConsumption * co2Gas) <= maxCO2)
            {
                double supply = Math.Min(remainingConsumption, maxCapacityGas);
                Q_gas[t] = supply;
                On_gas[t] = supply > 0;
                remainingConsumption -= supply;
                totalCost += supply * gasPrice;
                totalCO2 += supply * co2Gas;
            }

            if (remainingConsumption > 0 && totalCO2 + (remainingConsumption * co2Diesel) <= maxCO2)
            {
                double supply = Math.Min(remainingConsumption, maxCapacityDiesel);
                Q_diesel[t] = supply;
                On_diesel[t] = supply > 0;
                remainingConsumption -= supply;
                totalCost += supply * dieselPrice;
                totalCO2 += supply * co2Diesel;
            }

            if (remainingConsumption > 0)
            {
                Console.WriteLine($"Unable to meet consumption from {data[t].TimeFrom} to {data[t].TimeTo}");
            }
        }

        Console.WriteLine($"Total Cost: {totalCost}");
        Console.WriteLine($"Total CO2 Emissions: {totalCO2}");
        for (int t = 0; t < data.Count; t++)
        {
            Console.WriteLine($"Period {data[t].TimeFrom} to {data[t].TimeTo}: Electric = {Q_electric[t]}, Diesel = {Q_diesel[t]}, Gas = {Q_gas[t]}");
            Console.WriteLine($"Boiler Status: Electric = {On_electric[t]}, Diesel = {On_diesel[t]}, Gas = {On_gas[t]}");
        }
    }
}
*/