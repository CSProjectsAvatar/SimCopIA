
namespace DataClassHierarchy
{
    // interface IVisitorOf<T, R> {
    //     R Visit(T node);
    // }
    public class Visitor<R> {
        public R Visit<T>(T node){
            return ((dynamic)this).Visiting((dynamic)node);
        }
    }
}