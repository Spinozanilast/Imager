using System.Data;

namespace Imager.Processors.Processors;

public class Normalizer
{
    public DataTable NormalizeDataTable(DataTable dataTable)
    {
        var sum = 0.0;
        foreach (DataRow row in dataTable.Rows)
        {
            foreach (var item in row.ItemArray)
            {
                sum += Convert.ToDouble(item);
            }
        }

        var normalizedDataTable = dataTable.Copy();
        var rowLength = normalizedDataTable.Rows.Count;
        foreach (DataRow row in normalizedDataTable.Rows)
        {
            for (int i = 0; i < rowLength; i++)
            {
                row[i] = sum == 0 ? 0 : Convert.ToDouble(row[i]) / sum;
            }
        }

        return normalizedDataTable;
    }

    public double[,] NormalizeMatrixBySum(int[,] matrix, int decimalPlaces)
    {
        var rows = matrix.GetLength(0);
        var cols = matrix.GetLength(1);

        double sum = 0;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                sum += matrix[i, j];
            }
        }

        var normalizedMatrix = new double[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                var normalizedValue = sum == 0 ? 0 : (double)matrix[i, j] / sum;
                normalizedMatrix[i, j] = Math.Round(normalizedValue, decimalPlaces);
            }
        }

        return normalizedMatrix;
    }
}