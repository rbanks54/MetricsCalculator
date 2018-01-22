using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;

namespace MetricsCalculator
{
    // Use strategy pattern / interface to generate report
    // Use template pattern to allow changing of output format via subclassing. Base class handles running the
    // tree, subclass handles formatting the data by overriding the addToReport method.
    public class ABCMetricsReport<OutputType> : InOrderReporter, IReport<OutputType>
    {
        int depth = -1;
        IABCMetricsReportFormatter<OutputType> formatter;
        public ABCMetricsReport(IABCMetricsReportFormatter<OutputType> formatter)
        {
            this.formatter = formatter;
        }
        public OutputType GenerateReport(MetricsAccumulator metrics)
        {
            Visit(metrics.GetRoot());
            return this.formatter.GetFormattedOutput();
        }

        public override void Visit(MetricsAccumulationNode node)
        {
            depth++;
            SyntaxKind nodeKind = node.nodeReference.Kind();

            double abcCount, totalAbcCount;
            int assignments, branches, calls;

            node.TryGetDataItem(ABCStrategy.ABCScoreAccumulator, out abcCount);
            node.TryGetDataItem(ABCStrategy.AssignmentCount, out assignments);
            node.TryGetDataItem(ABCStrategy.BranchingCount, out branches);
            node.TryGetDataItem(ABCStrategy.CallCount, out calls);

            node.TryGetDataItem<double>(ABCStrategy.ABCScoreWithChildrenAccumulator, out totalAbcCount);

            formatter.AddItem(depth, node.Name, abcCount, totalAbcCount, assignments, branches, calls);

            base.Visit(node);
            depth--;
        }
    }
}
