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
#if !NO_LOCK_ASSIGNMENT
        private static readonly object dAssigning = new object();
        private static readonly object maAssigning = new object();
#endif

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
            for (int j = 0; j < H; j++)
            {
                double sum = 0.0;
                double c = 0.0;
                for (int i = 0; i < N; i++)
                {
                    double y = B[i] * (ME[i, j] + MZ[i, j]) - E[i] * (MM[i, j] + ME[i, j]) - c;
                    double t = sum + y;
                    c = t - sum - y;
                    sum = t;
                }

#if NO_LOCK_ASSIGNMENT
                D[j] = sum;
#else
                lock (dAssigning)
                {
                    D[j] = sum;
                }
#endif
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

            // MAH = a * (MEH + MZH) - ME * MMH
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < H; j++)
                {
                    double sum = a * (ME[i, j] + MZ[i, j]);
                    double c = 0.0;
                    for (int k = 0; k < N; k++)
                    {
                        double y = -ME[i, k] * MM[k, j] - c;
                        double t = sum + y;
                        c = t - sum - y;
                        sum = t;
                    }

#if NO_LOCK_ASSIGNMENT
                    MA[i, j] = sum;
#else
                    lock (maAssigning)
                    {
                        MA[i, j] = sum;
                    }
#endif
                }
            }

            // Signal: MA matrix ready to output
            maMatrixReady.Set();
        }

        private static void Thread2Func()
        {
            // Formula 1: D = В * (МE + MZ) - E * (MM + МE)
            for (int j = H; j < 2 * H; j++)
            {
                double sum = 0.0;
                double c = 0.0;
                for (int i = 0; i < N; i++)
                {
                    double y = B[i] * (ME[i, j] + MZ[i, j]) - E[i] * (MM[i, j] + ME[i, j]) - c;
                    double t = sum + y;
                    c = t - sum - y;
                    sum = t;
                }

#if NO_LOCK_ASSIGNMENT
                D[j] = sum;
#else
                lock (dAssigning)
                {
                    D[j] = sum;
                }
#endif
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

            // MAH = a * (MEH + MZH) - ME * MMH
            for (int i = 0; i < N; i++)
            {
                for (int j = H; j < 2 * H; j++)
                {
                    double sum = a * (ME[i, j] + MZ[i, j]);
                    double c = 0.0;
                    for (int k = 0; k < N; k++)
                    {
                        double y = -ME[i, k] * MM[k, j] - c;
                        double t = sum + y;
                        c = t - sum - y;
                        sum = t;
                    }

#if NO_LOCK_ASSIGNMENT
                    MA[i, j] = sum;
#else
                    lock (maAssigning)
                    {
                        MA[i, j] = sum;
                    }
#endif
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
