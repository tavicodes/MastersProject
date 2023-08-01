var lines = File.ReadLines("input.txt");
char[] separators = new char[] { ' ', ',' };
List<String> outputLines = new List<String>();

foreach (var line in lines)
{
    String newLine = line;
    if (line.StartsWith("\"id\":") || line.StartsWith("\"parent\":") || line.StartsWith("\"allow\":") || line.StartsWith("\"deny\":"))
    {
        String[] subs = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        if (Int32.Parse(subs[1]) != 0) subs[1] = (Int32.Parse(subs[1]) + 1).ToString();
        newLine = String.Concat(subs[0], ' ', subs[1]);
        if (!line.StartsWith("\"deny:\"")) newLine += ',';
    }
    outputLines.Add(newLine);
}

File.WriteAllLines("output.txt", outputLines.ToArray());