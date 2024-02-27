using System;
using System.IO;
using System.Text;

namespace Complex_Systems_Lab1
{
    public static class DataSerializer
    {
        private const string FILE_PARTS_DELIMITER = "\t";
        private const string NUMBERS_DELIMITER = "; ";

        public static StringBuilder SerializeVector(double[] vector)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < vector.Length; i++)
            {                
                sb.Append(vector[i]);
                if (i != vector.Length - 1) sb.Append(NUMBERS_DELIMITER);
            }

            return sb;
        }

        public static StringBuilder SerializeMatrix(double[,] matrix)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    sb.Append(matrix[i, j]);
                    if (j != matrix.GetLength(1) - 1) sb.Append(NUMBERS_DELIMITER);
                }

                if (i != matrix.GetLength(0) - 1) sb.Append(Environment.NewLine);
            }

            return sb;
        }

        public static void ReadVector(string filename, double[] vector)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), filename);
            string vectorFile = File.ReadAllText(path);
            string[] fileParts = vectorFile.Split(FILE_PARTS_DELIMITER);
            int vectorLength = int.Parse(fileParts[0]);
            string[] numberStrings = fileParts[1].Split(NUMBERS_DELIMITER);

            for (int i = 0; i < vectorLength; i++)
            {
                vector[i] = double.Parse(numberStrings[i]);
            }
        }

        public static void WriteVector(string filename, double[] vector)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), filename);
            StringBuilder vectorSb = SerializeVector(vector);
            File.WriteAllText(path, vector.Length.ToString() + FILE_PARTS_DELIMITER);
            File.AppendAllText(path, vectorSb.ToString());
        }

        public static void ReadMatrix(string filename, double[,] matrix)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), filename);
            string matrixFile = File.ReadAllText(path);
            string[] fileParts = matrixFile.Split(FILE_PARTS_DELIMITER);
            
            string[] sizeStrings = fileParts[0].Split(" ");
            int matrixHeight = int.Parse(sizeStrings[0]);
            int matrixWidth = int.Parse(sizeStrings[1]);

            string[] numberLines = fileParts[1].Split(Environment.NewLine);

            for (int i = 0; i < matrixHeight; i++)
            {
                string[] numberStrings = numberLines[i].Split(NUMBERS_DELIMITER);
                for (int j = 0; j < matrixWidth; j++)
                {
                    matrix[i, j] = double.Parse(numberStrings[j]);                    
                }
            }
        }

        public static void WriteMatrix(string filename, double[,] matrix)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), filename);
            StringBuilder matrixSb = SerializeMatrix(matrix);
            string sizesLine = $"{matrix.GetLength(0)} {matrix.GetLength(1)}";
            File.WriteAllText(path, sizesLine + FILE_PARTS_DELIMITER);
            File.AppendAllText(path, matrixSb.ToString());
        }
    }
}
