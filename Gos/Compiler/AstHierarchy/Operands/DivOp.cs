

using System;
using Compiler;
using Microsoft.Extensions.Logging;

namespace DataClassHierarchy
{
    public class DivOp:NumOp
    {
        ILogger<DivOp> _log;
        public DivOp(Expression left, Expression right, ILogger<DivOp> logger):base(left, right){
            _log = logger;
        }
        public override (bool, object) TryCompute(object left, object right){
            if(left is double l && right is double r){
                if(r == 0){
                    _log?.LogError("Line {line}, column {col}: can't divide by 0.", Token.Line, Token.Column);
                    return (false, null);
                }
                return (true, l / r);
            }
            return (false, null);
        }
    }
}
    