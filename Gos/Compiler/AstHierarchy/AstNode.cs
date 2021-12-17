
namespace DataClassHierarchy
{
    public abstract class AstNode : IValidable
    {
        public abstract bool Validate(Context context); 

    }
    public interface IValidable
    {
        bool Validate(Context context);
    }
}