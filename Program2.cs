using System;
using System.Diagnostics;
using System.Threading;

namespace Complex_Systems_Lab1
{
    public class Program2
    {
        // Startup consts
        private const int N = 100;
        private const int P = 2;
        private const int H = N / P;

        // Math objects
        private static double a;

        private static readonly double[] D = new double[N];
        private static readonly double[] E = new double[N];
        private static readonly double[] B = new double[N];

        private static readonly double[,] MA = new double[N, N];
        private static readonly double[,] ME = new double[N, N];
        private static readonly double[,] MM = new double[N, N];
        private static readonly double[,] MZ = new double[N, N];

        // Synchronization objects
        private static readonly object aCalculating = new object();

        private static readonly EventWaitHandle dVectorReady =
            new EventWaitHandle(false, EventResetMode.ManualReset);
        private static readonly EventWaitHandle a1Ready =
            new EventWaitHandle(false, EventResetMode.ManualReset);
        private static readonly EventWaitHandle a2Ready =
            new EventWaitHandle(false, EventResetMode.ManualReset);
        private static readonly EventWaitHandle maMatrixReady =
            new EventWaitHandle(false, EventResetMode.ManualReset);

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

        private static void Thread1Func()
        {
            // Formula 1: D = В * (МE + MZ) - E * (MM + МE)

            // MLH = МEH + MZH
            // MPH = MMH + МEH
            double[,] MLH = new double[N, H];
            double[,] MPH = new double[N, H];

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < H; j++)
                {
                    MLH[i, j] = ME[i, j] + MZ[i, j];
                    MPH[i, j] = MM[i, j] + ME[i, j];
                }
            }

            // DH = B * MLH - E * MPH
            for (int j = 0; j < H; j++)
            {
                for (int i = 0; i < N; i++)
                {
                    D[j] += B[i] * MLH[i, j] - E[i] * MPH[i, j];
                }
            }

            // Waiting for D vector to output
            dVectorReady.WaitOne();

            // T1 result output
            Console.WriteLine("D calculated:");
            Console.WriteLine(DataSerializer.SerializeVector(D).ToString());
            Console.WriteLine(Environment.NewLine);



            // Formula 2: MА = min(MM) * (ME + MZ) - ME * MM

            // ai = min(MMH)
            double ai = MM[0, 0];
            for (int i = 0; i < H; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    if (MM[i, j] < a)
                    {
                        a = MM[i, j];
                    }
                }
            }

            lock (aCalculating)
            {
                if (ai < a)
                {
                    a = ai;
                }
            }

            // Signal a1 ready, waiting for a2
            a1Ready.Set();
            a2Ready.WaitOne();

            // MKH = a * (MEH + MZH)
            // MNH = MEH * MMH
            double[,] MKH = new double[N, H];
            double[,] MNH = new double[N, H];

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < H; j++)
                {
                    MKH[i, j] = a * (ME[i, j] + MZ[i, j]);

                    for (int k = 0; k < N; k++)
                    {
                        MNH[i, j] += ME[i, k] * MM[k, j];
                    }
                }
            }

            // MAH = MKH - MNH
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < H; j++)
                {
                    MA[i, j] = MKH[i, j] - MNH[i, j];
                }
            }

            // Signal: MA matrix ready to output
            maMatrixReady.Set();
        }

        private static void Thread2Func()
        {
            // Formula 1: D = В * (МE + MZ) - E * (MM + МE)

            // ML = МE + MZ
            // MP = MM + МE
            double[,] MLH = new double[N, H];
            double[,] MPH = new double[N, H];

            for (int i = 0; i < N; i++)
            {
                for (int j = H; j < 2 * H; j++)
                {
                    MLH[i, j - H] = ME[i, j] + MZ[i, j];
                    MPH[i, j - H] = MM[i, j] + ME[i, j];
                }
            }

            // D = B * ML - E * MP
            for (int j = H; j < 2 * H; j++)
            {
                for (int i = 0; i < N; i++)
                {
                    D[j] += B[i] * MLH[i, j - H] - E[i] * MPH[i, j - H];
                }
            }

            // Signal: Vector D ready for output
            dVectorReady.Set();



            // Formula 2: MА = min(MM) * (ME + MZ) - ME * MM

            // ai = min(MMH)
            double ai = MM[H, 0];
            for (int i = H; i < 2 * H; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    if (MM[i, j] < a)
                    {
                        a = MM[i, j];
                    }
                }
            }

            lock (aCalculating)
            {
                if (ai < a)
                {
                    a = ai;
                }
            }

            // Signal a2 ready, waiting for a1
            a2Ready.Set();
            a1Ready.WaitOne();

            // MKH = a * (MEH + MZH)
            // MNH = MEH * MMH
            double[,] MKH = new double[N, H];
            double[,] MNH = new double[N, H];

            for (int i = 0; i < N; i++)
            {
                for (int j = H; j < 2 * H; j++)
                {
                    MKH[i, j - H] = a * (ME[i, j] + MZ[i, j]);

                    for (int k = 0; k < N; k++)
                    {
                        MNH[i, j - H] += ME[i, k] * MM[k, j];
                    }
                }
            }

            // MAH = MKH - MNH
            for (int i = 0; i < N; i++)
            {
                for (int j = H; j < 2 * H; j++)
                {
                    MA[i, j] = MKH[i, j - H] - MNH[i, j - H];
                }
            }

            // Waiting for MA matrix to output
            maMatrixReady.WaitOne();

            // T2 result output
            Console.WriteLine("MA calculated:");
            Console.WriteLine(DataSerializer.SerializeMatrix(MA).ToString());
            Console.WriteLine(Environment.NewLine);
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("Program2 started");

            bool isDataGenerate = args.Length > 0 ? args[0] == "--generate" : false;
            FillMathObjects(isDataGenerate);
            a = MM[0, 0];

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
            Console.WriteLine("Program2 finished");
            Console.ReadKey();
        }
    }
}
