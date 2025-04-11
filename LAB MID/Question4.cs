using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GrammarAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Grammar FIRST and FOLLOW Sets Calculator");
            Console.WriteLine("======================================");
            Console.WriteLine("Enter grammar rules (one per line). Enter an empty line to finish input.");
            Console.WriteLine("Format: non-terminal -> sequence1 | sequence2 | ...");
            Console.WriteLine("Example: E -> T X");
            Console.WriteLine("         X -> + T X | ε");
            Console.WriteLine("         T -> int | ( E )");
            
            // Read grammar rules from user
            List<string> inputRules = new List<string>();
            string input;
            
            while (!string.IsNullOrWhiteSpace(input = Console.ReadLine()))
            {
                inputRules.Add(input);
            }
            
            // Parse rules and check for left recursion
            var grammar = new Grammar();
            
            if (!grammar.ParseRules(inputRules))
            {
                Console.WriteLine("Error parsing grammar rules. Please check the format.");
                return;
            }
            
            // Check for left recursion
            if (grammar.HasLeftRecursion())
            {
                Console.WriteLine("Grammar invalid for top-down parsing.");
                return;
            }
            
            // Compute FIRST sets
            var firstSets = grammar.ComputeFirstSets();
            Console.WriteLine("\nFIRST Sets:");
            PrintSets(firstSets);
            
            // Compute FOLLOW sets
            var followSets = grammar.ComputeFollowSets(firstSets);
            Console.WriteLine("\nFOLLOW Sets:");
            PrintSets(followSets);
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
        
        static void PrintSets(Dictionary<string, HashSet<string>> sets)
        {
            foreach (var entry in sets)
            {
                Console.Write($"FIRST({entry.Key}) = {{ ");
                Console.Write(string.Join(", ", entry.Value));
                Console.WriteLine(" }");
            }
        }
    }
    
    class Grammar
    {
        // Dictionary to store the production rules
        private Dictionary<string, List<List<string>>> rules = new Dictionary<string, List<List<string>>>();
        private HashSet<string> nonTerminals = new HashSet<string>();
        private HashSet<string> terminals = new HashSet<string>();
        private string startSymbol = null;
        
        // Parse the input rules
        public bool ParseRules(List<string> inputRules)
        {
            if (inputRules.Count == 0)
                return false;
                
            // Regular expression to match grammar rules
            var rulePattern = new Regex(@"^(\w+)\s*->\s*(.+)$");
            
            foreach (var inputRule in inputRules)
            {
                var match = rulePattern.Match(inputRule);
                if (!match.Success)
                    return false;
                    
                string nonTerminal = match.Groups[1].Value.Trim();
                nonTerminals.Add(nonTerminal);
                
                // Set the first non-terminal as the start symbol
                if (startSymbol == null)
                    startSymbol = nonTerminal;
                
                // Split alternatives (separated by |)
                string rightSide = match.Groups[2].Value;
                string[] alternatives = rightSide.Split('|');
                
                List<List<string>> productions = new List<List<string>>();
                
                foreach (var alt in alternatives)
                {
                    // Split the alternative into symbols
                    List<string> symbols = new List<string>();
                    string[] tokens = alt.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    
                    foreach (var token in tokens)
                    {
                        if (token == "ε")
                        {
                            symbols.Add("ε"); // Epsilon
                        }
                        else
                        {
                            symbols.Add(token);
                            // If it's not a non-terminal, it's a terminal
                            if (!nonTerminals.Contains(token))
                                terminals.Add(token);
                        }
                    }
                    
                    productions.Add(symbols);
                }
                
                // Add or update the rule
                if (rules.ContainsKey(nonTerminal))
                    rules[nonTerminal].AddRange(productions);
                else
                    rules[nonTerminal] = productions;
            }
            
            // Verify all symbols in the rules are defined
            foreach (var entry in rules)
            {
                foreach (var production in entry.Value)
                {
                    foreach (var symbol in production)
                    {
                        if (symbol != "ε" && !nonTerminals.Contains(symbol) && !terminals.Contains(symbol))
                        {
                            Console.WriteLine($"Warning: Symbol '{symbol}' is used but not defined.");
                        }
                    }
                }
            }
            
            return true;
        }
        
        // Check if the grammar has left recursion
        public bool HasLeftRecursion()
        {
            foreach (var entry in rules)
            {
                string nonTerminal = entry.Key;
                
                // Direct left recursion
                foreach (var production in entry.Value)
                {
                    if (production.Count > 0 && production[0] == nonTerminal)
                    {
                        Console.WriteLine($"Direct left recursion found in rule: {nonTerminal} -> {string.Join(" ", production)}");
                        return true;
                    }
                }
                
                // Indirect left recursion (simplified check)
                HashSet<string> visited = new HashSet<string>();
                if (HasIndirectLeftRecursion(nonTerminal, nonTerminal, visited))
                {
                    Console.WriteLine($"Indirect left recursion detected involving {nonTerminal}");
                    return true;
                }
            }
            
            return false;
        }
        
        private bool HasIndirectLeftRecursion(string original, string current, HashSet<string> visited)
        {
            if (visited.Contains(current))
                return false;
                
            visited.Add(current);
            
            // Check each production of current non-terminal
            if (rules.ContainsKey(current))
            {
                foreach (var production in rules[current])
                {
                    if (production.Count > 0)
                    {
                        string first = production[0];
                        
                        // If first symbol is the original non-terminal, we found indirect left recursion
                        if (first == original && current != original)
                            return true;
                            
                        // If first symbol is a non-terminal, continue checking
                        if (nonTerminals.Contains(first) && first != current)
                        {
                            if (HasIndirectLeftRecursion(original, first, new HashSet<string>(visited)))
                                return true;
                        }
                    }
                }
            }
            
            return false;
        }
        
        // Compute FIRST sets for all non-terminals
        public Dictionary<string, HashSet<string>> ComputeFirstSets()
        {
            Dictionary<string, HashSet<string>> firstSets = new Dictionary<string, HashSet<string>>();
            
            // Initialize FIRST sets
            foreach (var terminal in terminals)
            {
                firstSets[terminal] = new HashSet<string> { terminal };
            }
            
            foreach (var nonTerminal in nonTerminals)
            {
                firstSets[nonTerminal] = new HashSet<string>();
            }
            
            bool changed;
            do
            {
                changed = false;
                
                foreach (var entry in rules)
                {
                    string nonTerminal = entry.Key;
                    
                    foreach (var production in entry.Value)
                    {
                        // If production is empty or epsilon, add epsilon to FIRST set
                        if (production.Count == 0 || (production.Count == 1 && production[0] == "ε"))
                        {
                            if (firstSets[nonTerminal].Add("ε"))
                                changed = true;
                            continue;
                        }
                        
                        // Process each symbol in the production
                        bool allCanDeriveEpsilon = true;
                        for (int i = 0; i < production.Count; i++)
                        {
                            string symbol = production[i];
                            
                            if (symbol == "ε")
                                continue;
                                
                            // Add FIRST(symbol) - {ε} to FIRST(nonTerminal)
                            bool epsilonInFirst = false;
                            foreach (var terminal in firstSets[symbol])
                            {
                                if (terminal != "ε")
                                {
                                    if (firstSets[nonTerminal].Add(terminal))
                                        changed = true;
                                }
                                else
                                {
                                    epsilonInFirst = true;
                                }
                            }
                            
                            // If the symbol cannot derive epsilon, stop here
                            if (!epsilonInFirst)
                            {
                                allCanDeriveEpsilon = false;
                                break;
                            }
                            
                            // If we've reached the last symbol and all can derive epsilon
                            if (i == production.Count - 1 && allCanDeriveEpsilon)
                            {
                                if (firstSets[nonTerminal].Add("ε"))
                                    changed = true;
                            }
                        }
                    }
                }
            } while (changed);
            
            return firstSets;
        }
        
        // Compute FOLLOW sets given the FIRST sets
        public Dictionary<string, HashSet<string>> ComputeFollowSets(Dictionary<string, HashSet<string>> firstSets)
        {
            Dictionary<string, HashSet<string>> followSets = new Dictionary<string, HashSet<string>>();
            
            // Initialize FOLLOW sets for all non-terminals
            foreach (var nonTerminal in nonTerminals)
            {
                followSets[nonTerminal] = new HashSet<string>();
            }
            
            // Add $ to FOLLOW(start symbol)
            followSets[startSymbol].Add("$");
            
            bool changed;
            do
            {
                changed = false;
                
                foreach (var entry in rules)
                {
                    string nonTerminal = entry.Key;
                    
                    foreach (var production in entry.Value)
                    {
                        for (int i = 0; i < production.Count; i++)
                        {
                            string symbol = production[i];
                            
                            // Skip terminals and epsilon
                            if (!nonTerminals.Contains(symbol))
                                continue;
                                
                            // Compute what follows the symbol in this production
                            bool allRemainderCanDeriveEpsilon = true;
                            
                            // Process all symbols after the current one
                            for (int j = i + 1; j < production.Count; j++)
                            {
                                string nextSymbol = production[j];
                                
                                // Skip epsilon
                                if (nextSymbol == "ε")
                                    continue;
                                    
                                // Add FIRST(nextSymbol) - {ε} to FOLLOW(symbol)
                                bool epsilonInNext = false;
                                foreach (var terminal in firstSets[nextSymbol])
                                {
                                    if (terminal != "ε")
                                    {
                                        if (followSets[symbol].Add(terminal))
                                            changed = true;
                                    }
                                    else
                                    {
                                        epsilonInNext = true;
                                    }
                                }
                                
                                // If nextSymbol cannot derive epsilon, stop here
                                if (!epsilonInNext)
                                {
                                    allRemainderCanDeriveEpsilon = false;
                                    break;
                                }
                            }
                            
                            // If all remaining symbols can derive epsilon or we're at the end
                            // Add FOLLOW(nonTerminal) to FOLLOW(symbol)
                            if (allRemainderCanDeriveEpsilon)
                            {
                                foreach (var terminal in followSets[nonTerminal])
                                {
                                    if (followSets[symbol].Add(terminal))
                                        changed = true;
                                }
                            }
                        }
                    }
                }
            } while (changed);
            
            return followSets;
        }
    }
}
