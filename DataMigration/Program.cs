using DataAccessLayer;
using DataAccessLayer.Models;
using DataMigration;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
		try
		{
            MigrationController mc = new MigrationController();
            await mc.DoWork();
            mc.ResetFailRows();
        }
		catch (Exception ex)
		{

			throw;
		}
        stopwatch.Stop();
        Console.WriteLine(stopwatch.Elapsed.ToString());
        Console.ReadLine();
    }
}
