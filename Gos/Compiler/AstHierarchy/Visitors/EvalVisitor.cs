
using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly ILogger<EvalVisitor> log;
        private readonly TextWriter _writer;
        private (bool Found, object Value) _returnFlag;

        public Context Context { get => stackC.Peek(); set => stackC.Push(value); }

        public EvalVisitor(Context global, ILogger<EvalVisitor> logger, TextWriter textWriter)
        {
            this.stackC = new Stack<Context>();
            Context = global;
            this.log = logger;
            _writer = textWriter;
        }
        
        public (bool, object) Visiting(AstNode node) {
            throw new Exception("Se llego por el visitor a la raiz, falta implementacion de Visiting"); // @audit esto es para probar, comentar dsp 
            // Se llego por el visitor a la raiz, no hay implementacion de Visiting en la clase hijo
            return (true, null);
        }  
    
        public object CheckBinar(BinaryExpr node) {
            var (lSuccess, lResult) = Visit(node.Left);
            if(!lSuccess){
                log.LogError("Left operand could not be Evalued");
                return null;
            }

            var (rSuccess, rResult) = Visit(node.Right);
            if(!rSuccess){
                log.LogError("Right operand could not be Evalued");
                return null;
            }
            return (lResult, rResult);
        }
        public (bool, object) Visiting(BinaryExpr node) {  
            var tResults = CheckBinar(node);
            if(tResults is null){
                return (false, null);
            }

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
                    log.LogError("Could not compute {node}", node);
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

            var childCtx = Context.CreateChildContext();
            foreach(var (arg, value) in arguments){
                childCtx.DefVariable(arg, value);
            } // pasando los argumentos con valores al nuevo contexto

            Context = childCtx; // cambiando el contexto
            
            var (f_success, _) = Visit(defFun.Body); // evaluando la funcion
            var returnedValue = _returnFlag.Value;
            _returnFlag = (false, null); // bajando la bandera de return
            
            stackC.Pop(); // destruyendo el ultimo contexto

            if(!f_success){
                log.LogError("Could not Evaluate {defFun}", defFun);
                return (false, null);
            }
            return (true, returnedValue);
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
        
        public (bool, object) Visiting(DefFun node){
            Context.SetFunc(node.Identifier, node.Arguments.Count, node);
            return (true, null);  
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
            _writer.WriteLine(result?.ToString());
            return (true, null); // @todo No tenemos soportado null(por eso el ? de arriba), hay q ver eso
        }

        public (bool, object) Visiting(ProgramNode node){
            return Visit(node.Statements);
        }
        
        public (bool, object) Visiting(IfStmt node){
            var (success, result) = Visit(node.Condition);
            if(!success || result is not bool){ // Error
                log.LogError("Could not Evaluate {st}", node.Condition);
                return (false, null);
            }
            if(result is bool r && r == true){
                Visit(node.Then);
            }

            return (true, null); 
        }
        

        /// <summary>
        /// Evalua una lista de statements y devuelve el resultado de la ultima
        /// </summary>
        public (bool, object) Visiting(IList<IStatement> statements){
            object lastR = null;

            foreach (var st in statements){
                var (success, result) = Visit(st);
                if(!success){
                    log.LogError("Could not Evaluate {st}", st);
                    return (false, null);
                }
                lastR = result;

                if(_returnFlag.Found) 
                    break;
            }
            return (true, null);
        }
        public (bool, object) Visiting(Return node){
            var (success, result) = Visit(node.Expr);
            if(!success){
                log.LogError("Could not Evaluate {node.Expr}", node.Expr);
                return (false, null);
            }
            _returnFlag = (true, result);
            return (true, result);    
        }
        
    }
}