using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

namespace MiniLanguageAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Mini-Language Variable Analyzer");
            Console.WriteLine("===============================");
            Console.WriteLine("Enter code sample (e.g., var a1 = 12@; float b2 = 3.14$$;):");
            
            string inputCode = Console.ReadLine();
            
         // If input is empty, use the example from the question
            if (string.IsNullOrWhiteSpace(inputCode))
            {
                inputCode = "var a1 = 12@; float b2 = 3.14$$;";
                Console.WriteLine("Using example input: " + inputCode);
            }
            string pattern = @"([abc]\w*\d+)\s*=\s*([^;]*?[^\w\s.][^;]*?);";
            
            var matches = Regex.Matches(inputCode, pattern);
            
            // Create list to store results
            List<VariableInfo> variables = new List<VariableInfo>();
            
            foreach (Match match in matches)
            {
                string varName = match.Groups[1].Value;
                string value = match.Groups[2].Value.Trim();
                
                // Extract special symbols
                string specialSymbols = ExtractSpecialSymbols(value);
                
                // Determine token type
                string tokenType = DetermineTokenType(value);
                
                if (!string.IsNullOrEmpty(specialSymbols))
                {
                    variables.Add(new VariableInfo(varName, specialSymbols, tokenType));
                }
            }
            
            // Display results in a table
            DisplayResultsTable(variables);
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
        
        static string ExtractSpecialSymbols(string value)
        {
            StringBuilder specialChars = new StringBuilder();
            
            foreach (char c in value)
            {
                if (!char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c) && c != '.')
                {
                    specialChars.Append(c);
                }
            }
            
            return specialChars.ToString();
        }
        
        static string DetermineTokenType(string value)
        {
            // Simple token type determination
            if (value.Contains("."))
            {
                return "FloatLiteral";
            }
            else if (int.TryParse(value.TrimEnd(value.Where(c => !char.IsDigit(c)).ToArray()), out _))
            {
                return "IntegerLiteral";
            }
            else
            {
                return "Unknown";
            }
        }
        
        static void DisplayResultsTable(List<VariableInfo> variables)
        {
            if (variables.Count == 0)
            {
                Console.WriteLine("\nNo matching variables found.");
                return;
            }
            
            // Table header
            Console.WriteLine("\nAnalysis Results:");
            Console.WriteLine("---------------------------------------------------------");
            Console.WriteLine("|  VarName  |  SpecialSymbol  |    Token Type    |");
            Console.WriteLine("---------------------------------------------------------");
            
            // Table rows
            foreach (var variable in variables)
            {
                Console.WriteLine($"|  {variable.Name,-8}  |  {variable.SpecialSymbols,-13}  |  {variable.TokenType,-16}  |");
            }
            
            Console.WriteLine("---------------------------------------------------------");
        }
    }
    
    class VariableInfo
    {
        public string Name { get; }
        public string SpecialSymbols { get; }
        public string TokenType { get; }
        
        public VariableInfo(string name, string specialSymbols, string tokenType)
        {
            Name = name;
            SpecialSymbols = specialSymbols;
            TokenType = tokenType;
        }
    }
}
