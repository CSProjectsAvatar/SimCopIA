
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Agents;
using Compiler;
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
            throw new Exception("Se llego por el visitor a la raiz, falta implementacion de Visiting"); // @remind esto es para probar, comentar
        }  
    
         public (bool, object) Visiting(BinaryExpr node) {  
            if(!CheckBinar(node, out var left, out var right)) {
                return (false, null);
            }
            var tResults = (left, right);

            switch (tResults) {  // @audit SEGURAMENT NECESITAREMOS UN DICCIONARIO PA SABER Q TIPOS ADMITE CADA OPERADOR EN SUS OPERANDOS
                case (double lNum, double rNum):
                    var (succ, result) = node.TryCompute(lNum, rNum);
                    if(!succ){
                        return (false, null);
                    }
                    return (true, result);
                
                default:
                    log.LogError(
                        "Line {line}, column {col}: only numbers can be computed by this operator. Left operand is {ltype} " +
                            "and right operand is {rtype}.",
                        node.Token.Line,
                        node.Token.Column,
                        Helper.GetType(left),
                        Helper.GetType(right));
                    return (false, null);
            }
        }
        

        public (bool, object) Visiting(Connection node) {  
            var eval = CheckConnection(node, out var left, out var rightList);
            if(!eval){
                return (false, null);
            }
            var (succ, result) = node.TryCompute(left, rightList);

            if(!succ){
                return (false, null);
            }

            return (true, null);
        }

        private bool CheckConnection(Connection node, out Agent left, out List<Agent> agList)
        {
            left = null;
            agList = new List<Agent>();
            // Checking Types of Agents
            foreach (var serv in node.Agents.Concat(new[] { node.LeftAgent }))
            {
                var varInstance = Context.GetVar(serv);
                var varType = Helper.GetType(varInstance);

                if (varType is not GosType.Server) {
                    log?.LogError(
                        "Line {line}, column {col}: variable '{serv}' has to be of type Server but it's of type {type}.",
                        node.Token.Line,
                        node.Token.Column,
                        serv,
                        varType);
                    return false;
                }
                agList.Add(varInstance as Agent);
            }
            left = agList.Last();
            agList.RemoveAt(agList.Count - 1);
            return true;
        }

        public (bool, object) Visiting(FunCall node){
            var exprValues = new List<object>();
            foreach(var expr in node.Args){
                var (success, result) = Visit(expr);
                if(!success){
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
                return (false, null);
            }
            return (true, returnedValue);
        }
        
        public (bool, object) Visiting(LetVar node){
            var (success, result) = Visit(node.Expr);
            if(!success){
                return (false, null);
            }
            if (result is Agent serv) {
                serv.ID = node.Identifier;
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
                return (false, null);
            }
            _writer.WriteLine(result?.ToString());
            return (true, null); // @todo No tenemos soportado null(por eso el ? de arriba), hay q ver eso
        }

        public (bool, object) Visiting(ProgramNode node){
            Context.Simulation = new Agents.Environment();

            var vis = Visit(node.Statements);
            // @audit DE JUGUETE
            Context.Simulation.AddSomeRequests();
            Context.Simulation.Run();

            log.LogInformation(
                Context.Simulation.solutionResponses.Aggregate(
                    $"Responses to environment:{System.Environment.NewLine}",
                    (accum, r) => accum + $"time:{r.responseTime} body:{r.body}{System.Environment.NewLine}"));
            return vis;
        }
        
        public (bool, object) Visiting(IfStmt node){
            var (success, result) = Visit(node.Condition);
            if(!success) {
                if (result is not bool) {  // @note ESTE CHEKEO NO HAC FALTA XQ LA SINTAXIS ASEGURA Q ESO SEA BOOLEANO. PERO CUAN2 SOPORTEMOS LLAMADOS D FUNC EN UNA COND HAY Q HACERLO OBLIGAO
                    log.LogError(
                        "Line {line}, column {col}: the conditional can't be a non-boolean expression.", 
                        node.Token.Line,
                        node.Token.Column);
                }
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
       public (bool, object) Visiting(Return node){
            var (success, result) = Visit(node.Expr);
            if(!success){
                return (false, null);
            }
            _returnFlag = (true, result);
            return (true, result);    
        }
        
        public (bool, object) Visiting(DistW node){
            var ds = new DistributionServer(Context.Simulation, null, new List<string>());
            Context.Simulation.AddAgent(ds);
            return (true, ds);       
        }

        // Auxiliars
        public (bool, object) Visiting(IList<IStatement> statements){
            object lastR = null;

            foreach (var st in statements){
                var (success, result) = Visit(st);
                if(!success){
                    return (false, null);
                }
                lastR = result;

                if(_returnFlag.Found) 
                    break;
            }
            return (true, null);
        }
        
        private bool CheckBinar(BinaryExpr node, out object leftResult, out object rightResult) {
            leftResult = rightResult = default;

            bool lSuccess;
            (lSuccess, leftResult) = Visit(node.Left);

            if(!lSuccess){
                //log.LogError(
                //    "Line {line}, column {col}: left operand could not be evaluated.", 
                //    node.Token.Line, 
                //    node.Token.Column);  @note CREO Q ESTO NO HAC FALTA, CADA CUAL LOGUEA CUAN2 C PART ALGO
                return false;
            }
            bool rSuccess;
            (rSuccess, rightResult) = Visit(node.Right);

            if(!rSuccess){
                //log.LogError(
                //    "Line {line}, column {col}: right operand could not be evaluated.", 
                //    node.Token.Line, 
                //    node.Token.Column);  @note CREO Q ESTO NO HAC FALTA, CADA CUAL LOGUEA CUAN2 C PART ALGO
                return false;
            }
            return true;
        }
       
    }
}