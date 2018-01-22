namespace MetricsCalculator
{
    public interface IABCMetricsReportFormatter<OutputType>
    {
        void AddItem(int depth, string name, double ABCMetric, double totalABCMetric, int assignments, int branches, int calls);

        OutputType GetFormattedOutput();
    }
}