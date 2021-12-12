
namespace DataClassHierarchy
{
    public abstract class Expression:AstNode
    {
        public abstract (bool Success, object Result) TryEval();

    }
}