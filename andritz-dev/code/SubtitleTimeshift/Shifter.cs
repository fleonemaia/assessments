using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SubtitleTimeshift
{
    public class Shifter
    {
        async static public Task Shift(Stream input, Stream output, TimeSpan timeSpan, Encoding encoding, int bufferSize = 1024, bool leaveOpen = false)
        {
            // To create and test regex:    https://regexr.com
            // first group:                 (?<first>\S+)
            // space arrow space:           \s-->\s
            // second group:                (?<second>\S+)
            var regexSRT = new Regex(@"(?<first>\S+)\s-->\s(?<second>\S+)");

            using (var streamWriter = new StreamWriter(output, encoding, bufferSize, leaveOpen))
            using (var streamReader = new StreamReader(input, encoding, true, bufferSize, leaveOpen))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    var isMatch = regexSRT.Match(line);
                    if (isMatch.Success)
                    {
                        line = ShiftTime(line, isMatch, 1, timeSpan);
                        line = ShiftTime(line, isMatch, 2, timeSpan);
                    }
                    await streamWriter.WriteLineAsync(line);
                }
            }
        }

        static private string ShiftTime(string line, Match isMatch, int group, TimeSpan timeSpan)
        {
            var time = isMatch.Groups[group].Value;
            var newTime = TimeSpan.Parse(time, new CultureInfo("id-ID")).Add(timeSpan).ToString("hh':'mm':'ss'.'fff", new CultureInfo("en-US"));
            return line.Replace(time, newTime);
        }
    }
}
