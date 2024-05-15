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
        double[] Q_electric = new double[data.Count];
        double[] Q_gas = new double[data.Count];
        double[] Q_oil = new double[data.Count];
        double[] Q_gasMotor = new double[data.Count];
        bool[] On_electric = new bool[data.Count];
        bool[] On_gas = new bool[data.Count];
        bool[] On_oil = new bool[data.Count];
        bool[] On_gasMotor = new bool[data.Count];

        for (int t = 0; t < data.Count; t++)
        {
            Q_electric[t] = 0;
            Q_gas[t] = 0;
            Q_oil[t] = 0;
            Q_gasMotor[t] = 0;
            On_electric[t] = false;
            On_gas[t] = false;
            On_oil[t] = false;
            On_gasMotor[t] = false;
        }

        double totalCost = 0;
        double totalCO2 = 0;

        for (int t = 0; t < data.Count; t++)
        {
            double consumption = data[t].HeatDemand;
            double remainingConsumption = consumption;

            double costElectricPerPeriod = costElectric * Math.Min(remainingConsumption, maxCapacityElectric);
            double costGasPerPeriod = costGas * Math.Min(remainingConsumption, maxCapacityGas);
            double costOilPerPeriod = costOil * Math.Min(remainingConsumption, maxCapacityOil);
            double costGasMotorPerPeriod = costGasMotor * Math.Min(remainingConsumption, maxCapacityGasMotor);

            double co2ElectricPerPeriod = co2Electric * Math.Min(remainingConsumption, maxCapacityElectric);
            double co2GasPerPeriod = co2Gas * Math.Min(remainingConsumption, maxCapacityGas);
            double co2OilPerPeriod = co2Oil * Math.Min(remainingConsumption, maxCapacityOil);
            double co2GasMotorPerPeriod = co2GasMotor * Math.Min(remainingConsumption, maxCapacityGasMotor);

            double[] costs = { costElectricPerPeriod, costGasPerPeriod, costOilPerPeriod, costGasMotorPerPeriod };
            double minCost = costs.Min();
            int minCostIndex = Array.IndexOf(costs, minCost);

            double[] co2Emissions = { co2ElectricPerPeriod, co2GasPerPeriod, co2OilPerPeriod, co2GasMotorPerPeriod };
            double maxCO2PerPeriod = 500;
            if (co2Emissions[minCostIndex] <= maxCO2PerPeriod)
            {
                switch (minCostIndex)
                {
                    case 0:
                        Q_electric[t] = Math.Min(remainingConsumption, maxCapacityElectric);
                        On_electric[t] = true;
                        totalCost += costElectricPerPeriod;
                        totalCO2 += co2ElectricPerPeriod;
                        break;
                    case 1:
                        Q_gas[t] = Math.Min(remainingConsumption, maxCapacityGas);
                        On_gas[t] = true;
                        totalCost += costGasPerPeriod;
                        totalCO2 += co2GasPerPeriod;
                        break;
                    case 2:
                        Q_oil[t] = Math.Min(remainingConsumption, maxCapacityOil);
                        On_oil[t] = true;
                        totalCost += costOilPerPeriod;
                        totalCO2 += co2OilPerPeriod;
                        break;
                    case 3:
                        Q_gasMotor[t] = Math.Min(remainingConsumption, maxCapacityGasMotor);
                        On_gasMotor[t] = true;
                        totalCost += costGasMotorPerPeriod;
                        totalCO2 += co2GasMotorPerPeriod;
                        break;


Unfinished;*/