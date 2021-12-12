
namespace DataClassHierarchy
{
    public class Number:Expression
    {
        public string Value { get; set; }

        public override (bool, object) TryEval()
        {
            if(double.TryParse(Value, out double result))
            {
                return (true, result);
            }
            return (false, null);
        }

        public override bool Validate(Context context)
        {
            return true;
        }
    }
}