using System;
using System.IO;
using System.Collections.Generic;

class SewerageBillCalculator
{
    static Dictionary<string, string> userCredentials = new Dictionary<string, string>();

    static void Main()
    {
        ShowWelcomeMessage();

        while (true)
        {
            PrintMainMenuOptions();

            string option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    CreateAccountScreen();
                    break;
                case "2":
                    LoginScreen();
                    break;
                case "3":
                    ExitApplication();
                    break;
                default:
                    PrintInvalidOptionMessage();
                    break;
            }
        }
    }

    static void CreateAccountScreen()
    {
        Console.Clear();
        ShowWelcomeMessage();
        Console.WriteLine("Account Creation");

        string name = GetInput("Enter your name: ");
        string meterNumber = GenerateMeterNumber();
        string password = GetPassword();

        userCredentials[meterNumber] = password;

        SaveAccountInfo(meterNumber, name);

        Console.WriteLine($"Account created successfully. Your meter number is {meterNumber}.");

        ReturnToMainMenu();
    }

    static void SaveAccountInfo(string meterNumber, string name)
    {
        string accountInfo = $"{meterNumber}:{name}";
        File.WriteAllText("account_info.txt", accountInfo);
    }

    static void LoginScreen()
    {
        Console.Clear();
        ShowWelcomeMessage();
        Console.WriteLine("Login");

        string meterNumber = GetInput("Enter your meter number: ");

        if (userCredentials.ContainsKey(meterNumber))
        {
            string password = GetPassword("Enter your password: ");
            if (userCredentials[meterNumber] == password)
            {
                Console.WriteLine("Login successful!");
                MainMenu(meterNumber);
            }
            else
            {
                Console.WriteLine("Invalid password. Please try again.");

                ReturnToMainMenu();
            }
        }
        else
        {
            Console.WriteLine("Meter number not found. Please check your meter number or create a new account.");

            ReturnToMainMenu();
        }
    }

    static void MainMenu(string meterNumber)
    {
        Console.Clear();
        ShowWelcomeMessage();
        double lastMonthReading = GetLastMonthReading(meterNumber);

        do
        {
            Console.WriteLine("Sewerage Bill Calculator");

            double waterConsumption = GetWaterConsumption();
            string usageType = GetUsageType();
            double rate = GetRate(usageType);
            double sewerageBill = CalculateSewerageBillAmount(waterConsumption, rate);

            GenerateInvoice(meterNumber, waterConsumption, usageType, rate, sewerageBill);

            StoreReadings(waterConsumption, usageType, meterNumber);

        } while (WantToCalculateAnotherBill());

        ReturnToMainMenu();
    }

    static void GenerateInvoice(string meterNumber, double waterConsumption, string usageType, double rate, double sewerageBill)
    {
        Console.Clear();
        ShowWelcomeMessage();

        string name = GetNameFromMeterNumber(meterNumber);

        Console.WriteLine("Invoice");
        Console.WriteLine($"User Name: {name}");
        Console.WriteLine($"Meter Number: {meterNumber}");
        Console.WriteLine($"Water Consumption: {waterConsumption} cubic meters");
        Console.WriteLine($"Usage Type: {usageType}");
        Console.WriteLine($"Rate: KES {rate:F2} per cubic meter");
        Console.WriteLine($"Total Bill: KES {sewerageBill:F2}");

        Console.WriteLine("\nInvoice generated successfully.");
        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }

    static double GetWaterConsumption()
    {
        double waterConsumption;
        do
        {
            Console.Write("Enter water consumption (in cubic meters): ");
        } while (!double.TryParse(Console.ReadLine(), out waterConsumption) || waterConsumption < 0);

        return waterConsumption;
    }

    static double CalculateSewerageBillAmount(double waterConsumption, double rate)
    {
        return waterConsumption * rate;
    }

    static void StoreReadings(double waterConsumption, string usageType, string meterNumber)
    {
        string filePath = $"{meterNumber}_readings.txt";

        using (StreamWriter writer = File.AppendText(filePath))
        {
            writer.WriteLine($"{DateTime.Now}: {usageType} - {waterConsumption} cubic meters");
        }

        Console.WriteLine("Meter reading stored successfully.");
    }

    static void ShowWelcomeMessage()
    {
        Console.WriteLine("\t\t\t\t\t\t\t************************************");
        Console.WriteLine("\t\t\t\t\t\t\t*                                  *");
        Console.WriteLine("\t\t\t\t\t\t\t*  WELCOME TO SEWERAGE CALCULATOR  *");
        Console.WriteLine("\t\t\t\t\t\t\t*                                  *");
        Console.WriteLine("\t\t\t\t\t\t\t************************************");
        Console.WriteLine();
    }

    static void ReturnToMainMenu()
    {
        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
        Console.Clear();
        ShowWelcomeMessage();
    }

    static string GenerateMeterNumber()
    {
        Random random = new Random();
        return $"METER{random.Next(100000, 999999)}";
    }

    static string GetPassword(string prompt = "Enter a password: ")
    {
        Console.Write(prompt);
        return GetHiddenInput();
    }

    static string GetHiddenInput(string prompt = "")
    {
        string input = "";
        ConsoleKeyInfo key;

        Console.Write(prompt);

        do
        {
            key = Console.ReadKey(true);

            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                input += key.KeyChar;
                Console.Write("*");
            }
            else if (key.Key == ConsoleKey.Backspace && input.Length > 0)
            {
                input = input.Substring(0, (input.Length - 1));
                Console.Write("\b \b");
            }
        } while (key.Key != ConsoleKey.Enter);

        Console.WriteLine();
        return input;
    }

    static double GetLastMonthReading(string meterNumber)
    {
        string filePath = $"{meterNumber}_readings.txt";

        if (File.Exists(filePath))
        {
            string[] allLines = File.ReadAllLines(filePath);

            if (allLines.Length > 0)
            {
                string lastLine = allLines[allLines.Length - 1];

                if (!string.IsNullOrEmpty(lastLine))
                {
                    string[] parts = lastLine.Split(':');

                    if (parts.Length == 2)
                    {
                        string[] readingParts = parts[1].Trim().Split('-');

                        if (readingParts.Length == 2 && double.TryParse(readingParts[1].Split(' ')[1], out double lastMonthReading))
                        {
                            return lastMonthReading;
                        }
                    }
                }
            }
        }

        return 0;
    }

    static string GetUsageType()
    {
        string usageType;
        do
        {
            Console.Write("Enter type of usage (residential/commercial/industrial): ");
            usageType = Console.ReadLine().ToLower();
        } while (string.IsNullOrWhiteSpace(usageType));

        return usageType;
    }

    static bool WantToCalculateAnotherBill()
    {
        Console.Write("Do you want to calculate another bill? (yes/no): ");
        return Console.ReadLine().ToLower() == "yes";
    }

    static string GetNameFromMeterNumber(string meterNumber)
    {
        string filePath = "account_info.txt";

        if (File.Exists(filePath))
        {
            string[] allLines = File.ReadAllLines(filePath);

            foreach (string line in allLines)
            {
                string[] parts = line.Split(':');

                if (parts.Length == 2 && parts[0].Trim() == meterNumber)
                {
                    return parts[1].Trim();
                }
            }
        }

        return "User Name Not Found";
    }

    static void PrintMainMenuOptions()
    {
        Console.WriteLine("1. Create Account");
        Console.WriteLine("2. Login");
        Console.WriteLine("3. Exit");

        Console.Write("Choose an option: ");
    }

    static void PrintInvalidOptionMessage()
    {
        Console.WriteLine("Invalid option. Please enter 1, 2, or 3.");
    }

    static void ExitApplication()
    {
        Console.WriteLine("Goodbye! Thank you for using the Sewerage Bill Calculator.");
        Environment.Exit(0);
    }

    static string GetInput(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine();
    }

    static double GetRate(string usageType)
    {
        switch (usageType)
        {
            case "residential":
                return 10.00;
            case "commercial":
                return 12.50;
            case "industrial":
                return 15.00;
            default:
                throw new ArgumentException("Invalid usage type. Please enter 'residential', 'commercial', or 'industrial'.");
        }
    }
}
