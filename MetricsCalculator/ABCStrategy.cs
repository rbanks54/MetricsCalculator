using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MetricsCalculator
{
    class ABCStrategy : IMetricsStrategy
    {
        public static readonly string ABCMetricData = "ABCMetric";
        public static readonly string TotalABCMetricData = "TotalABCMetric";
        public static readonly string AssignmentCount = "AssignmentCount";
        public static readonly string BranchingCount = "BranchingCount";
        public static readonly string CallCount = "CallCount";

        IDataStoreProvider dataProvider;

        SortedSet<SyntaxKind> linesToCount;

        /// <summary>
        /// ABC Metric Calculation
        /// </summary>
        /// <remarks>
        /// The ABC Metric is calculated based on the numbers of Assignments, Branches, and Calls in a method 
        /// and represents the software size of code (i.e. higher values require more cognitive processing to understand it).
        /// The original idea is from Jerry Fitzpatrick in 1997 - www.softwarerenovation.com/ABCMetric.pdf
        /// This approach is a slight tweak of the idea to add weightings and use slightly different terminology.
        /// 
        /// The calculation is pretty straightfoward
        /// 
        /// ABCValue = sqrt(Assignments^2 + Branches^2 + Calls^2)
        /// </remarks>


        List<SyntaxKind> BranchingSyntax = new List<SyntaxKind>
        {
            SyntaxKind.IfStatement,
            SyntaxKind.WhileStatement,
            SyntaxKind.DoStatement,
            SyntaxKind.ForStatement,
            SyntaxKind.ForEachStatement,
            SyntaxKind.SwitchKeyword,
            SyntaxKind.LogicalOrExpression,
            SyntaxKind.LogicalAndExpression,
        };

        public ABCStrategy()
        {
        }

        public void FinalizeSelf(MetricsAccumulationNode node)
        {
            int assignmentCount;
            int branchCount;
            int callCount;
            double abcCount;
            double childrenAbcCount = 0;

            node.TryGetDataItem<int>(AssignmentCount, out assignmentCount);
            node.TryGetDataItem<int>(BranchingCount, out branchCount);
            node.TryGetDataItem<int>(CallCount, out callCount);
            node.TryGetDataItem<double>(ABCMetricData, out abcCount);
            abcCount = Math.Sqrt((assignmentCount ^ 2) + (branchCount ^ 2) + (callCount ^ 2));
            node.SetDataItem<double>(ABCMetricData, abcCount);

            foreach (MetricsAccumulationNode child in node.children)
            {
                double childABCMetric = 0;
                if (child.TryGetDataItem<double>(TotalABCMetricData, out childABCMetric) ||
                    child.TryGetDataItem<double>(ABCMetricData, out childABCMetric))
                {
                    childrenAbcCount += childABCMetric;
                }
            }

            node.SetDataItem<double>(TotalABCMetricData, childrenAbcCount);
        }

        public void Process(SyntaxNode node)
        {
            IMetricsDataStore storage = dataProvider.GetDataStore();
            if (BranchingSyntax.Contains(node.Kind()))
            {
                int currentCount;
                storage.TryGetDataItem<int>(BranchingCount, out currentCount);
                storage.SetDataItem<int>(BranchingCount, ++currentCount);
            }
        }

        public void SetDataStoreProvider(IDataStoreProvider dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        public void InitializeSelf()
        {
            IMetricsDataStore storage = dataProvider.GetDataStore();
            storage.SetDataItem<double>(ABCMetricData, 0);
            storage.SetDataItem<int>(AssignmentCount, 0);
            storage.SetDataItem<int>(BranchingCount, 0);
            storage.SetDataItem<int>(CallCount, 0);
        }
    }
}

