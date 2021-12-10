

using System.Collections.Generic;
namespace DataClassHierarchy
{
    public class DefFun:Statement
    {
        public string Identifier { get; set; }
        public Expression Expr { get; set; }
        public List<string> Arguments { get; set; }
    }
}