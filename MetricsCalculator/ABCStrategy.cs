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
        public static readonly string ABCScoreAccumulator = "ABCScore";
        public static readonly string ABCScoreWithChildrenAccumulator = "ABCScoreWithChildren";
        public static readonly string AssignmentCount = "AssignmentCount";
        public static readonly string BranchingCount = "BranchingCount";
        public static readonly string CallCount = "CallCount";

        IDataStoreProvider dataProvider;

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

        List<SyntaxKind> AssignmentSyntax = new List<SyntaxKind>
        {
            SyntaxKind.SimpleAssignmentExpression,
            SyntaxKind.AddAssignmentExpression,
            SyntaxKind.AndAssignmentExpression,
            SyntaxKind.DivideAssignmentExpression,
            SyntaxKind.ExclusiveOrAssignmentExpression,
            SyntaxKind.LeftShiftAssignmentExpression,
            SyntaxKind.ModuloAssignmentExpression,
            SyntaxKind.MultiplyAssignmentExpression,
            SyntaxKind.OrAssignmentExpression,
            SyntaxKind.RightShiftAssignmentExpression,
            SyntaxKind.SubtractAssignmentExpression,
        };

        List<SyntaxKind> CallSyntax = new List<SyntaxKind>
        {
            SyntaxKind.InvocationExpression,
            SyntaxKind.ObjectCreationExpression,
            SyntaxKind.SimpleMemberAccessExpression
        };

        public ABCStrategy()
        {
        }

        public void FinalizeSelf(MetricsAccumulationNode node)
        {
            int assignmentCount;
            int branchCount;
            int callCount;

            node.TryGetDataItem<int>(AssignmentCount, out assignmentCount);
            node.TryGetDataItem<int>(BranchingCount, out branchCount);
            node.TryGetDataItem<int>(CallCount, out callCount);
            var ABCScore = Math.Sqrt(Math.Pow(assignmentCount,2) + Math.Pow(branchCount,2) + Math.Pow(callCount,2));
            node.SetDataItem<double>(ABCScoreAccumulator, ABCScore);

            if (node.children.Count > 0)
            {
                var ABCWithChildren = ABCScore;
                foreach (MetricsAccumulationNode child in node.children)
                {
                    double ABCScoreForChildren = 0;
                    if (child.TryGetDataItem<double>(ABCScoreWithChildrenAccumulator, out ABCScoreForChildren) ||
                        child.TryGetDataItem<double>(ABCScoreAccumulator, out ABCScoreForChildren))
                    {
                        ABCWithChildren += ABCScoreForChildren;
                    }
                }
                node.SetDataItem<double>(ABCScoreWithChildrenAccumulator, ABCWithChildren);
            }
        }

        public void Process(SyntaxNode node)
        {
            IMetricsDataStore storage = dataProvider.GetDataStore();
            var nodeKind = node.Kind();

            if (AssignmentSyntax.Contains(nodeKind))
            {
                IncrementOccurenceCount(AssignmentCount);
            }

            if (nodeKind == SyntaxKind.EqualsValueClause && node.Parent.IsKind(SyntaxKind.VariableDeclarator))
            {
                IncrementOccurenceCount(AssignmentCount);
            }

            if (BranchingSyntax.Contains(nodeKind))
            {
                IncrementOccurenceCount(BranchingCount);
            }

            if (CallSyntax.Contains(nodeKind))
            {
                IncrementOccurenceCount(CallCount);
            }
        }

        private void IncrementOccurenceCount(string counterName)
        {
            IMetricsDataStore storage = dataProvider.GetDataStore();
            int currentCount;
            storage.TryGetDataItem<int>(counterName, out currentCount);
            storage.SetDataItem<int>(counterName, ++currentCount);
        }

        public void SetDataStoreProvider(IDataStoreProvider dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        public void InitializeSelf()
        {
            IMetricsDataStore storage = dataProvider.GetDataStore();
            storage.SetDataItem<double>(ABCScoreAccumulator, 0);
            storage.SetDataItem<int>(AssignmentCount, 0);
            storage.SetDataItem<int>(BranchingCount, 0);
            storage.SetDataItem<int>(CallCount, 0);
        }
    }
}

