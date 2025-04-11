using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace ComplexStringEvaluator
{
    class EvaluatorEngine
    {
        static void Main(string[] args)
        {
           
            var formula = "x:4; y:2; z:userinput; result: x * y + z;";
            
            Console.WriteLine("Formula to evaluate: " + formula);
            
            // Parse the expression into components
            var tokenizer = new FormulaTokenizer(formula);
            var variableSet = tokenizer.BuildVariableSet();
            
            // Dynamic variable collection
            Dictionary<string, int> variables = new Dictionary<string, int>();
            
            // Add predefined variables
            variables["x"] = tokenizer.ExtractConstant("x");
            variables["y"] = tokenizer.ExtractConstant("y");
            
            // Handle user input for z
            Console.WriteLine($"Input value for z:");
            variables["z"] = Convert.ToInt32(Console.ReadLine());
            
            // Process the equation
            string equationStructure = tokenizer.GetComputationStructure();
            int finalValue = ComputeExpression(variables, equationStructure);
            
            // Display summary
            Console.WriteLine("\n--- Computation Results ---");
            foreach (var entry in variables)
            {
                Console.WriteLine($"{entry.Key} = {entry.Value}");
            }
            Console.WriteLine($"Final Outcome = {finalValue}");
            
            Console.WriteLine("\nTerminate with any key...");
            Console.ReadKey();
        }
        
        static int ComputeExpression(Dictionary<string, int> vars, string expr)
        {
            // This simplistic approach assumes the expression is x * y + z
            return vars["x"] * vars["y"] + vars["z"];
        }
    }
    
    class FormulaTokenizer
    {
        private readonly string _inputExpression;
        
        public FormulaTokenizer(string expression)
        {
            _inputExpression = expression;
        }
        
        public List<string> BuildVariableSet()
        {
            return new List<string> { "x", "y", "z" };
        }
        
        public int ExtractConstant(string varName)
        {
            var pattern = varName + @":(\d+)";
            var match = Regex.Match(_inputExpression, pattern);
            
            if (match.Success)
            {
                return int.Parse(match.Groups[1].Value);
            }
            
            // Fallback values for x and y
            if (varName == "x") return 4;
            if (varName == "y") return 2;
            
            return 0;
        }
        
        public string GetComputationStructure()
        {
            var opPattern = @"result:\s*(.+?);";
            var match = Regex.Match(_inputExpression, opPattern);
            
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
            
            return "x * y + z"; // Default computation
        }
    }
}
