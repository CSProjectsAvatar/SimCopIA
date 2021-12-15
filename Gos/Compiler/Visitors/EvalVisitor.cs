
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace DataClassHierarchy
{
    /// <summary>
    /// Evalua la clase pasada.
    /// </summary>
    public class EvalVisitor:Visitor<(bool, object)>
    {
        private Stack<Context> stackC;
        private readonly ILogger<EvalVisitor> _logger;
        private ILogger log;
        public Context Context { get => stackC.Peek(); set => stackC.Push(value); }

        public EvalVisitor(Context global, ILogger logger)
        {
            this.stackC = new Stack<Context>();
            Context = global;
            this.log = logger;
        }
        public (bool, object) Visiting(AstNode node) {
            // throw new Exception("Se llego por el visitor a la raiz, falta implementacion de Visiting"); 
            // Se llego por el visitor a la raiz, no hay implementacion de Visiting en la clase hijo
            return (true, null);
        }  
        public (bool, object) Visiting(NumOp node) {
            var (lSuccess, lResult) = Visit(node.Left);
            if(!lSuccess){
                log.LogError("Left operand could not be Evalued");
                return (false, null);
            }

            var (rSuccess, rResult) = Visit(node.Right);
            if(!rSuccess){
                log.LogError("Right operand could not be Evalued");
                return (false, null);
            }
            var tResults = (lResult, rResult);

            switch (tResults)
            {
                case (double lNum, double rNum):
                    var (succ, result) = node.TryCompute(lNum, rNum);
                    if(!succ){
                        log.LogError("Could not compute {lNum} {this} {rNum}", lNum, this, rNum);
                        return (false, null);
                    }
                    return (true, result);
                
                default:
                    log.LogError("Could not compute {lResult} {this} {rResult}", lResult, this, rResult);
                    return (false, null);
            }
        }
        
        public (bool, object) Visiting(FunCall node){
            var exprValues = new List<object>();
            foreach(var expr in node.Args){
                var (success, result) = Visit(expr);
                if(!success){
                    log.LogError("Could not Evaluate {expr}", expr);
                    return (false, null);
                }
                exprValues.Add(result);
            } // evaluando los argumentos

            // Apareandolos con sus Evaluaciones
            var defFun = Context.GetFunc(node.Identifier, node.Args.Count);
            var arguments = defFun.Arguments.Zip(exprValues, (arg, value) => (arg, value));

            var childCtx = new Context();
            foreach(var (arg, value) in arguments){
                childCtx.DefVariable(arg, value);
            } // pasando los argumentos con valores al nuevo contexto

            Context = childCtx;
            var (f_success, f_result) = Visit(defFun.Body); // evaluando la funcion
            stackC.Pop();

            if(!f_success){
                log.LogError("Could not Evaluate {defFun}", defFun);
                return (false, null);
            }
            return (true, f_result);
        }
        
        public (bool, object) Visiting(LetVar node){
            var (success, result) = Visit(node.Expr);
            if(!success){
                log.LogError("Could not Evaluate {node.Expr}", node.Expr);
                return (false, null);
            }
            Context.SetVar(node.Identifier, result);
            return (true, result);  
        }
    
        public (bool, object) Visiting(Variable node){
            var result = Context.GetVar(node.Identifier);
            if(result is null){
                throw new Exception("Una variable ya definida dio null, y no permitimos null");
            }
            return (true, result);
        }
    
        public (bool, object) Visiting(Number node){
            return (true, node.GetVal());
        }

        public (bool, object) Visiting(Print node){
            var (success, result) = Visit(node.Expr);
            if(!success){
                log.LogError("Could not Evaluate {node.Expr}", node.Expr);
                return (false, null);
            }
            log.LogInformation(result.ToString()); // @audit Como imprimimos?
            return (true, null); // @audit No tenemos soportado null creo, puede dar bateo
        }

        public (bool, object) Visiting(Program node){
            foreach (var st in node.Statements){
                var (success, result) = Visit(st);
                if(!success){
                    log.LogError("Could not Evaluate {st}", st);
                    return (false, null);
                }
            }
            return (true, null);    
        }
        
    }
}