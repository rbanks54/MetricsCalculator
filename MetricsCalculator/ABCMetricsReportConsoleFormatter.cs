using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsCalculator
{
    class ABCMetricsReportConsoleFormatter : IABCMetricsReportFormatter<string>
    {
        StringBuilder sb;
        bool outputDelievered;
        public ABCMetricsReportConsoleFormatter()
        {
            sb = new StringBuilder();
            outputDelievered = false;
        }

        public void AddItem(int depth, string name, double abcCount, double totalAbcCount)
        {
            if (outputDelievered)
            {
                return;
            }

            var indents = new String('\t', depth);
            var reportLine = $"{indents}{name}\tABC: {abcCount:0.##} \tTotal ABC: {totalAbcCount:0.##}";

            sb.AppendLine(reportLine);
        }

        public string GetFormattedOutput()
        {
            outputDelievered = true;
            return sb.ToString();
        }
    }
}
