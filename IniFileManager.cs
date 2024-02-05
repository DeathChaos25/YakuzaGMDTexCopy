using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class IniFile
{
    private readonly string filePath;
    private readonly Dictionary<string, string> keyValuePairs;

    public IniFile(string filePath)
    {
        this.filePath = filePath;
        keyValuePairs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        ReadIniFile();
    }

    public string TexturesDir => GetValue("TexturesDir", "DefaultTexturesDir");
    public string CommonDir => GetValue("CommonDir", "DefaultCommonDir");
    public string TargetDir => GetValue("TargetDir", "DefaultTargetDir");

    private string GetValue(string key, string defaultValue)
    {
        return keyValuePairs.TryGetValue(key, out var value) ? value : defaultValue;
    }

    private void ReadIniFile()
    {
        if (File.Exists(filePath))
        {
            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                var parts = line.Split('=');
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();
                    keyValuePairs[key] = value;
                }
            }
        }
        else
        {
            // Create the INI file with default values
            File.WriteAllLines(filePath, new[]
            {
                "TexturesDir=DefaultTexturesDir",
                "CommonDir=DefaultCommonDir",
                "TargetDir=DefaultTargetDir"
            });
        }
    }
}
