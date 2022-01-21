
namespace DataClassHierarchy
{
    ///<summary>
    /// Esta clase solo se utiliza cuando se hace un llamado a una variable ej. a = b + c
    ///</summary>
    public class Variable:Expression
    {
        public string Identifier { get; set; }

        public override bool Validate(Context context)
        {
            return context.CheckVar(Identifier);
        }
    }
}