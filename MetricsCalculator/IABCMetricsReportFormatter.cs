namespace MetricsCalculator
{
    public interface IABCMetricsReportFormatter<OutputType>
    {
        void AddItem(int depth, string name, double ABCMetric, double totalABCMetric);

        OutputType GetFormattedOutput();
    }
}