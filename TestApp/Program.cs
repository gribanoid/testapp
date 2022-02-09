using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace TestApp
{
    class Program
    {
        public static string inputPath = @"C:\Users\grish\Рабочий стол\input.txt";
        public static string outputPath = @"C:\Users\grish\Рабочий стол\output.csv";
        private static readonly ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["connectionString"];
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine($"Нажмите Enter, если хотите выполнить sql запрос из {inputPath}");
                Console.ReadLine();
                string queryString = ReadFromFileAsync(inputPath).Result;
                if (string.IsNullOrEmpty(queryString))
                    continue;
                bool success = SqlExecuterAsync(queryString, outputPath).Result;
                if (success)
                    Console.WriteLine("Sql запрос был успешно выполнен");
                    Console.WriteLine("-------------------------------");
            }
        }


        public static async Task<bool> SqlExecuterAsync(string script, string outPath)
        {
            try
            {
                using (SqlConnection connection = new(settings.ConnectionString))
                {
                    await connection.OpenAsync().ConfigureAwait(false);
                    return WriteToCSV(new SqlCommand(script, connection).ExecuteReader(), outPath);
                }   
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception occurred while working with the database: " + e.Message);
                return false;
            }
            
            
        }

        public static bool WriteToCSV(IDataReader reader, string outPath)
        {
            try
            {
                List<string> lines = new List<string>();
                string headerLine = "";
                if (reader.Read())
                {
                    string[] columns = new string[reader.FieldCount];
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        columns[i] = reader.GetName(i);
                    }
                    headerLine = string.Join(",", columns);
                    lines.Add(headerLine);
                }
                while (reader.Read())
                {
                    object[] values = new object[reader.FieldCount];
                    reader.GetValues(values);
                    lines.Add(string.Join(",", values));
                }
                File.WriteAllLines(outPath, lines);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception occurred while writing to output.csv: " + e.Message);
                return false;
            }
        }





        public static async Task<string> ReadFromFileAsync(string inPath) 
        {
            string script = "";
            try
            {
                using (StreamReader sr = new StreamReader(inPath))
                {        
                    while (true)
                    {
                        string fileString = await sr.ReadLineAsync().ConfigureAwait(false);
                        if (string.IsNullOrEmpty(fileString))
                            break;
                        script += fileString + " ";
                    }
                }
                return script;
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception occurred while reading input.txt: " + e.Message);
                return "";
            }
        }
    }
    
}
