
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace PatternSequenceValidator
{
    // Record for tracking lexical elements
    public record LexicalItem
    {
        public string Identifier { get; init; }
        public string Category { get; init; }
        public string Content { get; init; }
        public int Position { get; init; }

        public string FormatInfo() => $"{Identifier} -> {Category} -> {Content} @ line {Position}";
    }

    class Analyzer
    {
        // Storage for valid lexical elements
        private readonly List<LexicalItem> _lexicalStorage = new();
        private int _positionTracker = 1;

        static void Main()
        {
            var analyzer = new Analyzer();
            Console.WriteLine("Mirrored Sequence Pattern Recognition Engine");
            Console.WriteLine("-------------------------------------------");
            Console.WriteLine("Input patterns for analysis (type 'TERMINATE' to finish):");

            string userInput;
            while ((userInput = Console.ReadLine()) != "TERMINATE")
            {
                analyzer.EvaluateSequence(userInput);
                analyzer._positionTracker++;
            }

            // Present collected data
            analyzer.GenerateReport();

            Console.WriteLine("\nPress any key to finish...");
            Console.ReadKey();
        }

        void EvaluateSequence(string sequence)
        {
            // Extract components using expression pattern
            var patternMatch = Regex.Match(sequence, @"(\w+)\s+(\w+)\s*=\s*([^;]+);");
            
            if (patternMatch.Success)
            {
                string category = patternMatch.Groups[1].Value;
                string identifier = patternMatch.Groups[2].Value;
                string content = patternMatch.Groups[3].Value.Trim();

                // Verify if identifier contains mirror patterns
                string mirrorSegment = DetectMirrorSequence(identifier, 3);
                
                if (!string.IsNullOrEmpty(mirrorSegment))
                {
                    // Record the verified item
                    _lexicalStorage.Add(new LexicalItem
                    {
                        Identifier = identifier,
                        Category = category,
                        Content = content,
                        Position = _positionTracker
                    });
                    
                    Console.WriteLine($"Validated: {identifier} (mirror pattern detected: '{mirrorSegment}')");
                }
                else
                {
                    Console.WriteLine($"Rejected: {identifier} (no mirror sequence of minimum length 3)");
                }
            }
            else
            {
                Console.WriteLine("Invalid syntax. Expected format: \"category identifier = content;\"");
            }
        }

        string DetectMirrorSequence(string input, int minimumLength)
        {
            // Algorithm to identify mirror sequence patterns
            for (int startPosition = 0; startPosition < input.Length; startPosition++)
            {
                // Evaluate all potential sequence lengths
                for (int sequenceLength = minimumLength; startPosition + sequenceLength <= input.Length; sequenceLength++)
                {
                    bool isMirrorPattern = true;
                    string candidateSequence = input.Substring(startPosition, sequenceLength);
                    
                    // Verify if sequence reads the same forward and backward
                    for (int charIndex = 0; charIndex < sequenceLength / 2; charIndex++)
                    {
                        if (candidateSequence[charIndex] != candidateSequence[sequenceLength - charIndex - 1])
                        {
                            isMirrorPattern = false;
                            break;
                        }
                    }
                    
                    if (isMirrorPattern)
                    {
                        return candidateSequence;
                    }
                }
            }
            
            return null;
        }

        void GenerateReport()
        {
            Console.WriteLine("\nValidated Pattern Repository:");
            Console.WriteLine("=================================================");
            Console.WriteLine("| Identifier     | Category  | Content    | Pos |");
            Console.WriteLine("=================================================");
            
            foreach (var item in _lexicalStorage)
            {
                Console.WriteLine($"| {item.Identifier,-14} | {item.Category,-9} | {item.Content,-10} | {item.Position,-3} |");
            }
            
            Console.WriteLine("=================================================");
            
            if (!_lexicalStorage.Any())
            {
                Console.WriteLine("Repository is empty. No valid patterns found.");
            }
        }
    }
}
