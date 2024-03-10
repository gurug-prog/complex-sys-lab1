using System;
using System.Diagnostics;
using System.Threading;

namespace Complex_Systems_Lab1
{
    public class Program1
    {
        // Startup consts
        private const int N = 100;

        // Math objects
        private static readonly double[] E = new double[N];
        private static readonly double[] B = new double[N];

        private static readonly double[,] ME = new double[N, N];
        private static readonly double[,] MM = new double[N, N];
        private static readonly double[,] MZ = new double[N, N];

        private static void FillMathObjects(bool isDataGenerate)
        {
            if (isDataGenerate)
            {
                NumberGenerator.FillVector(E);
                NumberGenerator.FillVector(B);

                NumberGenerator.FillMatrix(ME);
                NumberGenerator.FillMatrix(MM);
                NumberGenerator.FillMatrix(MZ);

                DataSerializer.WriteVector("E.txt", E);
                DataSerializer.WriteVector("B.txt", B);

                DataSerializer.WriteMatrix("ME.txt", ME);
                DataSerializer.WriteMatrix("MM.txt", MM);
                DataSerializer.WriteMatrix("MZ.txt", MZ);
            }
            else
            {
                DataSerializer.ReadVector("E.txt", E);
                DataSerializer.ReadVector("B.txt", B);

                DataSerializer.ReadMatrix("ME.txt", ME);
                DataSerializer.ReadMatrix("MM.txt", MM);
                DataSerializer.ReadMatrix("MZ.txt", MZ);
            }
        }

        // Formula 1: D = В * (МE + MZ) - E * (MM + МE)
        private static void Thread1Func()
        {
            double[] D = new double[N];
            for (int j = 0; j < N; j++)
            {
                for (int i = 0; i < N; i++)
                {
                    D[j] += B[i] * (ME[i, j] + MZ[i, j]) - E[i] * (MM[i, j] + ME[i, j]);
                }
            }

            // T1 result output
            Console.WriteLine("D calculated:");
            Console.WriteLine(DataSerializer.SerializeVector(D).ToString());
            Console.WriteLine(Environment.NewLine);
        }

        // Formula 2: MА = min(MM) * (ME + MZ) - ME * MM
        private static void Thread2Func()
        {
            // a = min(MM)
            double a = MM[0, 0];
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    if (MM[i, j] < a)
                    {
                        a = MM[i, j];
                    }
                }
            }

            // MA = a * (ME + MZ) - ME * MM
            double[,] MA = new double[N, N];
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    MA[i, j] = a * (ME[i, j] + MZ[i, j]);

                    for (int k = 0; k < N; k++)
                    {
                        MA[i, j] -= ME[i, k] * MM[k, j];
                    }
                }
            }

            // T2 result output
            Console.WriteLine("MA calculated:");
            Console.WriteLine(DataSerializer.SerializeMatrix(MA).ToString());
            Console.WriteLine(Environment.NewLine);
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("Program1 started");

            bool isDataGenerate = args.Length > 0 ? args[0] == "--generate" : false;
            FillMathObjects(isDataGenerate);

            Thread t1 = new Thread(Thread1Func);
            Thread t2 = new Thread(Thread2Func);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();

            sw.Stop();

            Console.WriteLine("Elapsed time: " + sw.Elapsed);
            Console.WriteLine("Program1 finished");
            Console.ReadKey();
        }
    }
}
