using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;

namespace Astnchrony_Multithreading_WPFApp
{
    //TASK 1
    //Create a windowed application that generates a set of prime numbers within the range specified by the user.
    //If the lower bound is not specified, the process starts from 2. 
    //If the upper bound is not specified, generation continues until the application is closed.
    //Use threading mechanism.The numbers should be displayed in the window interface.

    //TASK 2
    //Add a thread to the first task that generates a set of Fibonacci numbers.
    //The numbers should be displayed in the window interface.

    //TASK 3
    //Add buttons to the second task for the complete stop of each of the threads.
    //One button for one thread. 
    //If the user presses the stop button, the thread completely terminates its operation.

    //TASK 4
    //Add buttons to the third task for pausing and resuming each of the threads.
    //For example, the user can pause the generation of Fibonacci numbers by pressing a button.
    //Resuming the generation is possible after pressing another button.

    //TASK 5
    //Add the ability to perform a full restart of threads with new boundaries to the fourth task.

    public partial class MainWindow : Window
    {
        private volatile bool stopPrime = false;
        private volatile bool stopFibonacci = false;

        private volatile bool pausePrime = false; 
        private volatile bool pauseFibonacci = false;

        private Thread primeThread;
        private Thread fibThread;

        private ManualResetEvent primeThreadFinished = new ManualResetEvent(true);
        private ManualResetEvent fibThreadFinished = new ManualResetEvent(true);

        public MainWindow()
        {
            InitializeComponent();
        }

        private object primeLock = new object(); // Lock object for prime generation
        private object fibLock = new object();   // Lock object for Fibonacci generation

        private void GeneratePrimes(int lower, int upper)
        {
            List<int> primes = new List<int>();

            for (int num = lower; num <= upper || upper == int.MaxValue; num++)
            {
                if (stopPrime) return;

                bool isPrime = true;
                for (int divisor = 2; divisor <= Math.Sqrt(num); divisor++)
                {
                    if (num % divisor == 0)
                    {
                        isPrime = false;
                        break;
                    }
                }

                if (isPrime)
                {
                    lock (primeLock)
                    {
                        primes.Add(num);

                        // Add a check for pausePrime
                        while (pausePrime)
                        {
                            Monitor.Wait(primeLock);
                        }

                        Dispatcher.Invoke(() => primeListBox.Items.Add(num));
                    }
                }
            }
        }

        private void GenerateFibonacci(int count)
        {
            int a = 0, b = 1;

            for (int i = 0; i < count; i++)
            {
                if (stopFibonacci) return;

                int temp = a;
                a = b;
                b = temp + b;

                lock (fibLock)
                {
                    Dispatcher.Invoke(() => fibonacciListBox.Items.Add(a));
                }
            }
        }

        private void GeneratePrimesAndFibonacci(int lowerBound, int upperBound)
        {
            primeThread = new Thread(() => GeneratePrimes(lowerBound, upperBound));
            fibThread = new Thread(() => GenerateFibonacci(20));

            primeThread.Start();
            fibThread.Start();
        }

        private void PausePrimeThread_Click(object sender, RoutedEventArgs e)
        {
            pausePrime = true;
        }

        private void ResumePrimeThread_Click(object sender, RoutedEventArgs e)
        {
            lock (primeLock)
            {
                pausePrime = false;
                Monitor.PulseAll(primeLock);
            }
        }

        private void PauseFibonacciThread_Click(object sender, RoutedEventArgs e)
        {
            pauseFibonacci = true;
        }

        private void ResumeFibonacciThread_Click(object sender, RoutedEventArgs e)
        {
            lock (fibLock)
            {
                pauseFibonacci = false;
                Monitor.PulseAll(fibLock);
            }
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            int lowerBound = string.IsNullOrEmpty(lowerBoundTextBox.Text) ? 2 : int.Parse(lowerBoundTextBox.Text);
            int upperBound = string.IsNullOrEmpty(upperBoundTextBox.Text) ? int.MaxValue : int.Parse(upperBoundTextBox.Text);

            GeneratePrimesAndFibonacci(lowerBound, upperBound);
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            primeListBox.Items.Clear();
            fibonacciListBox.Items.Clear();
        }

        private void StopPrimeThread_Click(object sender, RoutedEventArgs e)
        {
            stopPrime = true;
        }

        private void StopFibonacciThread_Click(object sender, RoutedEventArgs e)
        {
            stopFibonacci = true;
        }

        private void RestartThreads(int lowerBound, int upperBound)
        {
            stopPrime = true;
            stopFibonacci = true;

            primeThreadFinished.WaitOne();
            fibThreadFinished.WaitOne();

            primeListBox.Items.Clear();
            fibonacciListBox.Items.Clear();

            primeThread = new Thread(() =>
            {
                GeneratePrimes(lowerBound, upperBound);
                primeThreadFinished.Set(); 
            });

            fibThread = new Thread(() =>
            {
                GenerateFibonacci(20);
                fibThreadFinished.Set(); 
            });

            primeThread.Start();
            fibThread.Start();
        }

        private void RestartThreads_Click(object sender, RoutedEventArgs e)
        {
            int lowerBound = string.IsNullOrEmpty(lowerBoundTextBox.Text) ? 2 : int.Parse(lowerBoundTextBox.Text);
            int upperBound = string.IsNullOrEmpty(upperBoundTextBox.Text) ? int.MaxValue : int.Parse(upperBoundTextBox.Text);

            RestartThreads(lowerBound, upperBound);
        }
    }
}
