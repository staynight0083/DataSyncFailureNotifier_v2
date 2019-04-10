using System;
using System.IO;
using System.Data.SqlClient;
using System.Net.Mail;

//Console project .Net Framework
namespace DataSyncFailureNotifier_v2
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");
            //Console.ReadLine();
            DateTime dt = new DateTime();  //DateTiem struct
            try
            {
                string[] temp = File.ReadAllLines("LastRunDate.txt");  //File.ReadAllLines() needs System.IO, open the LastRunDate.txt file and read the line into the array
                dt = new DateTime(int.Parse(temp[0]), int.Parse(temp[1]), int.Parse(temp[2]), int.Parse(temp[3]), int.Parse(temp[4]), 0);
            }
            catch (FileNotFoundException)  //if "LastRunDate.txt" does not exist
            {
                File.Create("LastRunDate.txt");  //Create the file LastRunDate.txt
                Console.WriteLine("File created."); //Display the message "File created."
            }
            catch (Exception) //if other exceition
            {
                Console.WriteLine("Error opening file.");
            }


            long lastLogNumber = 0, currentLogNumber = 0;
            try
            {
                lastLogNumber = int.Parse(File.ReadAllText("LastLogNumber.txt"));
            }
            catch (FileNotFoundException)
            {
                File.Create("LastLogNumber.txt");
                Console.WriteLine("File Created.");
            }
            catch (Exception)
            {
                Console.WriteLine("Error opening file.");
            }

            string credentials = "user id=activity_read; password=yaQ6staf; server=192.168.110.204; database=MSCS_IDEC_PRD"; //There is a dedicatec activity_read account on the database MSCS_IDEC_PRD

            SqlConnection conn = new SqlConnection(credentials); //Setup connection to the SQL database. Requires System.Data.SqlClient
            SqlCommand sql = new SqlCommand(
                @"SELECT TOP 1 NavisionLastLogNumber
                    FROM [MSCS_IDEC_PRD].[dbo].[Nav_SynchronizingEvents]
                   WHERE NavisionLastLogNumber IS NOT NULL
                ORDER BY Nav_SynchronizingEventID DESC", conn); //SqlCommand throw query string and connection (conn) to the database

            try { conn.Open(); }  //Open the connection
            catch (Exception) { Console.WriteLine("DB Connection Error!"); }

            SqlDataReader read = sql.ExecuteReader(); //SqlDataReader
            while (read.Read())
                currentLogNumber = long.Parse(read[0].ToString());

            try { conn.Close(); }
            catch (Exception) { }

            File.WriteAllText("LastLogNumber.txt", currentLogNumber.ToString());

            DateTime now = DateTime.Now;
            string date =
                now.Year.ToString() + '\n' +
                now.Month.ToString() + '\n' +
                now.Day.ToString() + '\n' +
                now.Hour.ToString() + '\n' +
                now.Minute.ToString();
            TimeSpan diff = now.Subtract(dt);  //TimeSpan struct represent a time interval
                                               //Calculate the difference between current time (now) and the dt

            File.WriteAllText("LastRunDate.txt", date);
            Console.WriteLine("Current:\t" + currentLogNumber);
            Console.WriteLine("Previous:\t" + lastLogNumber);

            string d = diff.TotalMinutes.ToString();
            d = d.Substring(0, d.IndexOf('.') + 3);

            if (currentLogNumber == lastLogNumber) //If the log number is not moving i.e. currentLogNumber == lastLogNumber
                mail("ping@idec.com", "all_it@idec.com", "Datasync Notification", "The data sync has not moved in a while.\n\nLast check:" + d + "minutes ago.\nLast Navision ID: " + currentLogNumber.ToString() + "\n\nIf Saturday or Sunday please Ignore the warning!!"); //mail function is as below

            //Console.ReadLine();
        }

        public static void mail(string sender, string recipient, string subject, string body)
        {
            MailMessage letter = new MailMessage(sender, recipient, subject, body);  //Represents an email sent by SmtpClient class. Requires System.Net.Mail
            SmtpClient mailman = new SmtpClient("localhost"); //SmtpClient allows program to send email by using the SMTP protocal
            try
            {
                mailman.UseDefaultCredentials = false;
                mailman.Host = "192.168.100.106";  //??
                mailman.Port = 25;
                //mailman.EnableSsl = true;
                Console.Write("Sending message:\n" +
                    "To     : " + recipient + '\n' +
                    "From   : " + sender + '\n' +
                    "Subject: " + subject + '\n' +
                    "__________________________MESSAGE BODY CONTENTS_________________________\n" + 
                    body +
                    "________________________________________________________________________\n");
                mailman.Send(letter); //SmteClient.Sened() send MailMessage "letter" as an email

            }
            catch(Exception e)
            {
                Console.Write("There was a problem sending the email message.\n\n" + e.Message + "\n\n" + e.Source + "\n\n" + e.StackTrace);
            }
        }
    }
}
