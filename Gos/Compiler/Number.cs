
namespace DataClassHierarchy
{
    public class Number:Expression
    {
        public string Value { get; set; }

        public override int Eval()
        {
            throw new System.NotImplementedException(); 
        }

        public override bool Validate(Context context)
        {
            throw new System.NotImplementedException();
        }
    }
}