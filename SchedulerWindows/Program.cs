using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32.TaskScheduler;
using MySql.Data.MySqlClient;

namespace SchedulerWindows
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectString = "server=localhost;port=3306;username=root;password=mysql;database=kursovaja;";
            string query = $"SELECT * FROM backuplist where Name = '{args[0]}'";
            List<string> data = new List<string>();

            MySqlConnection myConnection = new MySqlConnection(connectString);
            myConnection.Open();

            MySqlCommand command = new MySqlCommand(query, myConnection);
            MySqlDataReader reader = command.ExecuteReader();

            reader.Read();
            for(int i = 0; i < 6; i++)
            {
                data.Add(reader[i].ToString());

            }

            reader.Close();
            myConnection.Close();

            if (int.Parse(data[5]) < 5)
            {
                string commands = $@"D:
                                 cd D:\Ampps\mysql\bin
                                 mysqldump -uroot -pmysql {data[2]} > {data[3]}\{args[0]}-{int.Parse(data[5]) + 1}.sql";

                createProcess(commands);

                myConnection.Open();
                query = $"update backuplist set BackupCount = {int.Parse(data[5]) + 1} where Name = '{args[0]}'";
                command = new MySqlCommand(query, myConnection);
                command.ExecuteNonQuery();
                myConnection.Close();
            }

            else
            {
                System.IO.File.Delete($@"{data[3]}\{args[0]}-1.sql");
                for (int i = 2; i <= 5; i++)
                {
                    System.IO.File.Move($@"{data[3]}\{args[0]}-{i}.sql", $@"{data[3]}\{args[0]}-{i-1}.sql");
                }
                
                string commands = $@"D:
                                 cd D:\Ampps\mysql\bin
                                 mysqldump -uroot -pmysql {data[2]} > {data[3]}\{args[0]}-5.sql";

                createProcess(commands);
            }

            void createProcess(string commands)
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        RedirectStandardInput = true,
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Hidden
                    }
                };
                process.Start();

                using (StreamWriter pWriter = process.StandardInput)
                {
                    if (pWriter.BaseStream.CanWrite)
                    {
                        foreach (var line in commands.Split('\n'))
                            pWriter.WriteLine(line);
                    }
                }
            }
        }
    }
}
