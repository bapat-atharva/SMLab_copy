using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using UnityEditor;
using UnityEngine;

public class CSVReaderv2
{
    public Dictionary<string, List<CsvRecordv2>> ReadData()
    {
        string csvFilePath = "Assets/Scripts/CSV/datav3.csv";

        List<string> lines = new List<string> {"Afghanistan", "Albania", "Algeria", "Angola", "Argentina", "Armenia", "Australia", "Austria", "Azerbaijan" };
        using (var reader = new StreamReader(csvFilePath))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            // Read CSV and get records
            var filteredRecords = csv.GetRecords<CsvRecordv2>().ToList();

            //foreach (var record in records)
            //{
            //    if (record.Year == null) record.Year = 0;
            //    if (record.LifeExpectancy == null) record.LifeExpectancy = 0;
            //    if (record.InfantDeaths == null) record.InfantDeaths = 0;
            //}

            // var filteredRecords = records.Where(r => lines.Contains(r.Country)).OrderBy(r => r.Area).Reverse().ToList();

            filteredRecords = NormalizeRecords(filteredRecords);

            // Group by ColumnA
            var groupedData = filteredRecords
                .GroupBy(r => r.Country)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Sort grouped records by the specified column
            foreach (var key in groupedData.Keys.ToList())
            {
                groupedData[key] = groupedData[key].OrderBy(r => r.Year).ToList().Take(50).ToList();
            }

            // Display grouped data
            //foreach (var group in groupedData)
            //{
            //    Debug.Log($"Group: {group.Key}");
            //    foreach (var record in group.Value)
            //    {
            //        Debug.Log($"  {record.Country}, {record.LifeExpectancy}, {record.Year}, {record.InfantDeaths}");
            //    }
            //}

            return groupedData;
        }
    }

    List<CsvRecordv2> NormalizeRecords(List<CsvRecordv2> records)
    {
        // Get min and max values for each column
        float minB = records.Min(r => r.Year);
        float maxB = records.Max(r => r.Year);
        float minC = records.Min(r => r.LifeExpectancy);
        float maxC = records.Max(r => r.LifeExpectancy);
        float minD = records.Min(r => r.InfantDeaths);
        float maxD = records.Max(r => r.InfantDeaths);

        float minE = records.Min(e => e.BirthRate);
        float maxE = records.Max(e => e.BirthRate);
        float minF = records.Min(e => e.Population);
        float maxF = records.Max(e => e.Population);

        // Normalize each record
        foreach (var record in records)
        {
            //record.Year = Normalize(record.Year, minB, maxB);
            record.LifeExpectancy = Normalize(record.LifeExpectancy, minC, maxC);
            record.InfantDeaths = Normalize(record.InfantDeaths, minD, maxD);
            record.BirthRate = Normalize(record.BirthRate, minE, maxE);
            record.Population = Normalize(record.Population, minF, maxF);

        }

        return records;
    }

    float Normalize(float value, float min, float max)
    {
        return (value - min) / (max - min);
    }
}

public class CsvRecordv2
{
    public string Country { get; set; }

    [Optional]
    [Default(0.0f)]
    public float LifeExpectancy { get; set; }
    public float Year { get; set; }
    public float InfantDeaths { get; set; }

    [Optional]
    [Default(0.0f)]
    public float BirthRate { get; set; }

    [Optional]
    [Default(0.0f)]
    public float Population { get; set; }
}
