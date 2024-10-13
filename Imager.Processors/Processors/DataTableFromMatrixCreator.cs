using System.Data;

namespace Imager.Processors.Processors;

public class DataTableFromMatrixCreator
{
    public static DataTable ConvertMatrixToDataTable<T>(T[,] matrix)
    {
        var dataTable = new DataTable();

        for (int i = 0; i < matrix.GetLength(1); i++)
        {
            dataTable.Columns.Add(i.ToString());
        }

        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            var row = dataTable.NewRow();
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                row[j] = matrix[i, j];
            }
            dataTable.Rows.Add(row);
        }

        return dataTable;
    }
}