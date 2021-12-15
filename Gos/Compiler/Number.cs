
using System;

namespace DataClassHierarchy
{
    public class Number:Expression
    {
        public string Value { get; set; }

        public double GetVal()
        {
            if(double.TryParse(Value, out double result)) {
                return result;
            }
            else {
                throw new Exception("Invalid number"); // Aki no debemos llegar nunca por nuestro Lexer
            }
        }

        public override bool Validate(Context context)
        {
            return true;
        }
    }
}