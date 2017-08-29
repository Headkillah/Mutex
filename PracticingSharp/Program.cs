using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

public class mutexTest
{
    private static List<String> filenames = new List<String>();
    private static Mutex mut = new Mutex();

    public static void Main(String[] args)
    {
        mutexTest mtest = new mutexTest();

        Thread readThread = new Thread(new ThreadStart(checkFiles));
        readThread.Name = "Reading Thread";
        readThread.Start();

        //The main thread allows the reading thread to start firstly
        Thread.Sleep(1000);

        Thread delThread = new Thread(new ThreadStart(deleteFiles));
        delThread.Name = "Deleting Thread";
        delThread.Start();

        Console.ReadLine();

    }

    private static void checkFiles()
    {
        bool over = true;
        try
        {
            Console.WriteLine(Thread.CurrentThread.Name + " is waiting for the mutex");
            mut.WaitOne();
            Console.WriteLine(Thread.CurrentThread.Name + " has acquired the mutex. Reading started\n");
            while (over)
            {
                try
                {
                    var files = from file in Directory.EnumerateFiles(@"c:\Test", "*.txt", SearchOption.TopDirectoryOnly)
                                select new
                                {
                                    File = file
                                };

                    foreach (var f in files)
                    {
                        if (!filenames.Contains(f.File))
                        {
                            Console.WriteLine("{0} content: ", f.File);
                            using (StreamReader sr = new StreamReader(f.File))
                            {
                                String line = sr.ReadToEnd();
                                Console.WriteLine(line + "\n");
                                sr.Close();
                            }
                            filenames.Add(f.File);
                        }
                        if (f.File.Contains("last.txt"))
                        {
                            over = false;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("The file could not be read:");
                    Console.WriteLine(e.Message);
                }
                Thread.Sleep(1000);
            }
        }
        finally
        {
            Console.WriteLine("The \'last\' file was found. Therefore the mutex is released from the " + Thread.CurrentThread.Name + "\n");
            mut.ReleaseMutex();
        }
    }

    private static void deleteFiles()
    {
        try
        {
            Console.WriteLine(Thread.CurrentThread.Name + " is waiting for the mutex\n");
            mut.WaitOne();
            Console.WriteLine(Thread.CurrentThread.Name + " has acquired the mutex");

            Console.WriteLine("deleteReading started...");
            try
            {
                var files = from file in Directory.EnumerateFiles(@"c:\Test", "*.txt", SearchOption.TopDirectoryOnly)
                            select new
                            {
                                File = file
                            };

                foreach (var f in files)
                {
                    File.Delete(f.File);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
            Console.WriteLine("deleteReading ...over");

        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception on return from deleteReading " +
            "\r\n\tMessage: {0}", ex.Message);
        }
        finally
        {
            mut.ReleaseMutex();
            Console.WriteLine(Thread.CurrentThread.Name + " has released the mutex");
        }

    }

    ~mutexTest()
    {
        {
            mut.Dispose();
        }
    }
}