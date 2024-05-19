using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

class DataEntry //Columns format in the csv file
{
    public DateTime TimeFrom { get; set; }
    public DateTime TimeTo { get; set; }
    public double HeatDemand { get; set; }
    public double ElectricityPrice { get; set; }
}

class HeatingOptimizer
{
    public List<DataEntry> WinterData { get; private set; } //Winter List
    public List<DataEntry> SummerData { get; private set; } //Summer List
 
    public void Read()
    {
        WinterData = new List<DataEntry>();
        SummerData = new List<DataEntry>();

        using (var reader = new StreamReader(@"datacsv.csv"))
        {
            reader.ReadLine(); //Skip header with definitions
            reader.ReadLine(); //Skip sub-header with more data definitions for the header

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(';');

                StoreData(values);
            }
        }
    }

    private void StoreData(string[] values) //Thanks to the person who did the csv read file function in the main program
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

    public void Optimize(bool isWinter) //Optimizer 
    {
        var data = isWinter ? WinterData : SummerData;

        //Boiler specifications extracted from the Main program
        double maxCapacityElectric = 8;  //MW
        double maxCapacityGas = 5;       //MW
        double maxCapacityOil = 4;       //MW
        double maxCapacityGasMotor = 3.6;//MW
        double maxElectricityGasMotor = 2.7; //MWh

        double co2Electric = 0;          //kg/MWh
        double co2Gas = 215;             //kg/MWh
        double co2Oil = 265;             //kg/MWh
        double co2GasMotor = 640;        //kg/MWh

        //Production costs (DKK/MWh)
        double costElectric = 50;
        double costGas = 500;
        double costOil = 700;
        double costGasMotor = 1100;

        //Gas Motor electricity production rate
        double gasMotorElectricityOutputRate = 0.5; //MWh(electricity)/MWh(th)

        //Optimizer variables
        double totalCost = 0;
        double totalCO2 = 0;
        double totalMoneySaved = 0;
        double totalRevenue = 0;
        double maxCO2 = 500; //Maximum allowed CO2 emissions per period (example, can be changed)

        foreach (var entry in data)
        {
            double remainingHeatDemand = entry.HeatDemand;
            double periodCost = 0;
            double periodCO2 = 0;
            double periodElectricityProduced = 0;
            double periodElectricityUsed = 0;
            double periodElectricityRevenue = 0;
            double periodElectricityCostReduction = 0;

            // Boiler selection based on cost and then on CO2 emissions
            var boilers = new List<(double maxCapacity, double cost, double co2, string name, double electricityOutputRate)>
            {
                (maxCapacityElectric, costElectric, co2Electric, "Electric", 0),
                (maxCapacityGas, costGas, co2Gas, "Gas", 0),
                (maxCapacityOil, costOil, co2Oil, "Oil", 0),
                (maxCapacityGasMotor, costGasMotor, co2GasMotor, "Gas Motor", gasMotorElectricityOutputRate)
            };

            //Sorting boilers by cost and then by CO2 emissions
            boilers = boilers.OrderBy(b => b.cost).ThenBy(b => b.co2).ToList();

            // First we need to operate the gas motor in the background to produce electricity that we can then sell
            double gasMotorHeatProduction = Math.Min(remainingHeatDemand, maxCapacityGasMotor);
            double gasMotorElectricityProduction = gasMotorHeatProduction * gasMotorElectricityOutputRate;

            //Limit the electricity production to the maximum limit
            if (gasMotorElectricityProduction > maxElectricityGasMotor)
            {
                gasMotorElectricityProduction = maxElectricityGasMotor;
                gasMotorHeatProduction = gasMotorElectricityProduction / gasMotorElectricityOutputRate;
            }

            periodElectricityProduced += gasMotorElectricityProduction;
            remainingHeatDemand -= gasMotorHeatProduction;
            periodCost += gasMotorHeatProduction * costGasMotor;
            periodCO2 += gasMotorHeatProduction * co2GasMotor;

            Console.WriteLine($"{entry.TimeFrom} to {entry.TimeTo}: Using Gas Motor Boiler");
            Console.WriteLine($"Allocated Heat: {gasMotorHeatProduction} MW, Cost: {gasMotorHeatProduction * costGasMotor} DKK, CO2: {gasMotorHeatProduction * co2GasMotor} kg, Electricity Output: {gasMotorElectricityProduction} MWh");

            //Use remaining demand across other boilers
            foreach (var boiler in boilers)
            {
                if (remainingHeatDemand <= 0)
                    break;

                double allocatedHeat = Math.Min(remainingHeatDemand, boiler.maxCapacity);
                double allocatedCost = allocatedHeat * boiler.cost;
                double allocatedCO2 = allocatedHeat * boiler.co2;

                if (boiler.name != "Gas Motor")
                {
                    periodCost += allocatedCost;
                    periodCO2 += allocatedCO2;
                    remainingHeatDemand -= allocatedHeat;

                    Console.WriteLine($"{entry.TimeFrom} to {entry.TimeTo}: Using {boiler.name} Boiler");
                    Console.WriteLine($"Allocated Heat: {allocatedHeat} MW, Cost: {allocatedCost} DKK, CO2: {allocatedCO2} kg");
                }
            }

            //Use produced electricity for the electric boiler
            double electricityUsedByElectricBoiler = Math.Min(periodElectricityProduced, maxCapacityElectric);
            double electricBoilerCostReduction = electricityUsedByElectricBoiler * entry.ElectricityPrice;
            periodCost -= electricBoilerCostReduction; //Reduce cost by the amount of electricity used internally
            periodElectricityCostReduction += electricBoilerCostReduction;
            periodElectricityUsed += electricityUsedByElectricBoiler;

            //Calculate the revenue from selling remaining electricity
            double electricitySold = periodElectricityProduced - electricityUsedByElectricBoiler;
            double electricityRevenue = electricitySold * entry.ElectricityPrice;
            periodElectricityRevenue += electricityRevenue;

            //Output the electricity usage and revenue information
            Console.WriteLine($"Electricity Produced: {periodElectricityProduced} MWh, Electricity Used: {periodElectricityUsed} MWh, Electricity Sold: {electricitySold} MWh, Revenue from Sold Electricity: {electricityRevenue} DKK");

            if (remainingHeatDemand > 0)
            {
                Console.WriteLine($"Unable to meet full heat demand from {entry.TimeFrom} to {entry.TimeTo} within CO2 constraints.");
            }

            totalMoneySaved += periodElectricityCostReduction;
            totalRevenue += periodElectricityRevenue;
            periodCost -= periodElectricityRevenue;
            totalCost += periodCost;
            totalCO2 += periodCO2;
        }

        //Outputing results
        Console.WriteLine($"Total Cost: {totalCost} DKK");
        Console.WriteLine($"Total CO2 Emissions: {totalCO2} kg");
        Console.WriteLine($"Total Money Saved from Using Gas Motor Electricity: {totalMoneySaved} DKK");
        Console.WriteLine($"Total Revenue from Sold Electricity: {totalRevenue} DKK");
    }

    static void Main()
    {
        HeatingOptimizer optimizer = new HeatingOptimizer();
        optimizer.Read();

        Console.WriteLine("Choose the period for optimization: ");
        Console.WriteLine("1. Winter");
        Console.WriteLine("2. Summer");
        int choice = int.Parse(Console.ReadLine());

        bool isWinter = (choice == 1);
        optimizer.Optimize(isWinter);
    }
}
