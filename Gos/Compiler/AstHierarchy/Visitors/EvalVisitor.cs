
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Agents;
using Compiler;
using Compiler.AstHierarchy;
using Compiler.AstHierarchy.Operands;
using Compiler.AstHierarchy.Statements;
using Microsoft.Extensions.Logging;

namespace DataClassHierarchy
{
    /// <summary>
    /// Evalua la clase pasada.
    /// </summary>
    public class EvalVisitor:Visitor<(bool, object)>
    {
        private Stack<Context> stackC;
        private readonly ILogger<EvalVisitor> _log;
        private readonly TextWriter _writer;
        private (bool Found, object Value) _returnFlag;

        public Context Context { get => stackC.Peek(); set => stackC.Push(value); }

        public EvalVisitor(Context global, ILogger<EvalVisitor> logger, TextWriter textWriter)
        {
            this.stackC = new Stack<Context>();
            Context = global;
            this._log = logger;
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
                    var (succ, result) = VisitBinExpr(node, lNum, rNum);
                    if(!succ){
                        return (false, null);
                    }
                    return (true, result);
                
                default:
                    _log.LogError(
                        "Line {line}, column {col}: only numbers can be computed by this operator. Left operand is {ltype} " +
                            "and right operand is {rtype}.",
                        node.Token.Line,
                        node.Token.Column,
                        Helper.GetType(left),
                        Helper.GetType(right));
                    return (false, null);
            }
        }

        private (bool Success, object Result) VisitBinExpr(BinaryExpr node, double lNum, double rNum) {
            return ((dynamic)this).Visiting((dynamic)node, lNum, rNum);
        }

        public (bool Success, object Result) Visiting(AddOp node, double lNum, double rNum) {
            return node.TryCompute(lNum, rNum);
        }

        public (bool Success, object Result) Visiting(DivOp node, double lNum, double rNum) {
            return node.TryCompute(lNum, rNum);
        }

        public (bool Success, object Result) Visiting(MultOp node, double lNum, double rNum) {
            return node.TryCompute(lNum, rNum);
        }

        public (bool Success, object Result) Visiting(SubOp node, double lNum, double rNum) {
            return node.TryCompute(lNum, rNum);
        }

        public (bool Success, object Result) Visiting(GreaterThanOp node, double lNum, double rNum) {
            return node.TryCompute(lNum, rNum);
        }

        public (bool Success, object Result) Visiting(LessThanOp node, double lNum, double rNum) {
            return node.TryCompute(lNum, rNum);
        }

        public (bool Success, object Result) Visiting(EqEqOp node, double lNum, double rNum) {
            return node.TryCompute(lNum, rNum);
        }

        public (bool Success, object Result) Visiting(RestOfDivOp node, double lNum, double rNum) {
            if (rNum == 0) {
                _log.LogError(
                    "Line {l}, column {c}: right operand can't be zero.",
                    node.Token.Line,
                    node.Token.Column);
                return (false, null);
            }
            return (true, lNum % rNum);
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
                    _log?.LogError(
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

        public (bool, object) Visiting(VarAssign node) {
            var (succ, value) = Visit(node.NewValueExpr);
            if (!succ) {
                return (false, null);
            }
            var type = Helper.GetType(Context.GetVar(node.Variable));
            var newType = Helper.GetType(value);

            if (type != newType) {
                _log.LogError(
                    "Line {l}, column {c}: types mismatch. Type {type} was expected but {valType} is given.",
                    node.Token.Line,
                    node.Token.Column,
                    type,
                    newType);
                return (false, null);
            }
            Context.SetVar(node.Variable, value);
            return (true, null);
        }

        public (bool, object) Visiting(ListIdxSetAst node) {
            List<object> list = null;
            int idxToUse = -1;
            var newListObj = Context.GetVar(node.RootListName); ;

            foreach (var idxExpr in node.Idxs) {
                // chekean2 tipo de la lista
                var newListType = Helper.GetType(newListObj);

                if (newListType != GosType.List) {
                    _log.LogError(
                        "Line {l}, column {c}: '{id}' must be a list but it's a {idType} instead.",
                        node.Token.Line,
                        node.Token.Column,
                        node.RootListName,
                        newListType);
                    return (false, null);
                }
                list = newListObj as List<object>;

                var (idxSucc, idxObj) = Visit(idxExpr);

                if (!idxSucc) {
                    return (false, null);
                }
                if (!IdxValid(idxObj, list.Count, node.Token.Line, node.Token.Column)) {
                    return (false, null);
                }
                idxToUse = (int)(double)idxObj - 1;
                newListObj = list[idxToUse];
            }
            var (succ, value) = Visit(node.NewValueExpr);

            if (!succ) {
                return (false, null);
            }
            var valType = Helper.GetType(value);
            var expectedType = Helper.GetType(list[0]);

            if (valType != expectedType) {
                _log.LogError(
                    "Line {l}, column {c}: new value must be of type {expect} but it's of type {act} instead.",
                    node.NewValueExpr.Token.Line,
                    node.NewValueExpr.Token.Column,
                    expectedType,
                    valType);
                return (false, null);
            }
            list[idxToUse] = value;

            return (true, null);
        }

        private bool IdxValid(object idxObj, int listCount, uint line, uint column) {
            // checkean2 q el i'ndic sea un #
            var idxType = Helper.GetType(idxObj);

            if (idxType != GosType.Number) {
                _log.LogError(
                    "Line {l}, column {c}: index must be a number, but it's of type {type} instead.",
                    line,
                    column,
                    idxType);
                return false;
            }
            // chekean2 q el i'ndic sea entero
            var idxDouble = (double)idxObj;
            var idxInt = (int)idxDouble;

            if (idxDouble - idxInt > double.Epsilon) {
                _log.LogError(
                    "Line {l}, column {c}: index can't be a fractional number.",
                    line,
                    column);
                return false;
            }
            if (idxInt <= 0 || idxInt > listCount) {
                _log.LogError(
                    "Line {l}, column {c}: index must be greater than 0 and less or equal than the list size.",
                    line,
                    column);
                return false;
            }
            return true;
        }

        public (bool, object) Visiting(ListIdxGetAst node) {
            var (succ, val) = Visit(node.Left);

            if (!succ) {
                return (false, null);
            }
            var type = Helper.GetType(val);

            if (type != GosType.List) {
                _log.LogError(
                    "Line {l}, column {c}: expression must be a list but it's a {idType} instead.",
                    node.Token.Line,
                    node.Token.Column,
                    type);
                return (false, null);
            }
            var list = val as List<object>;
            var (idxSucc, idxObj) = Visit(node.Index);

            if (!idxSucc) {
                return (false, null);
            }
            if (!IdxValid(idxObj, list.Count, node.Token.Line, node.Token.Column)) {
                return (false, null);
            }
            return (true, list[(int)(double)idxObj - 1]);
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
            _writer.WriteLine(GosObjToString(result));
            return (true, null); // @todo No tenemos soportado null(por eso el ? de arriba), hay q ver eso
        }

        private string GosObjToString(object obj) {
            return Helper.GetType(obj) is GosType.List
                ? $"[{string.Join(", ", (obj as List<object>).Select(elem => GosObjToString(elem)))}]"
                : obj?.ToString();
        }

        public (bool, object) Visiting(ProgramNode node){
            Context.Simulation = new Agents.Environment();

            var vis = Visit(node.Statements);
            
            if (vis.Item1) {  // evaluacio'n exitosa
                // @audit DE JUGUETE
                Context.Simulation.AddSomeRequests();
                Context.Simulation.Run();

                _log.LogInformation(
                    Context.Simulation.solutionResponses.Aggregate(
                        $"Responses to environment:{System.Environment.NewLine}",
                        (accum, r) => accum + $"time:{r.responseTime} body:{r.body}{System.Environment.NewLine}"));
            }
            return vis;
        }
        
        public (bool, object) Visiting(IfStmt node){
            int idx = 0;  // el i'ndic de la 1ra cond q es true

            foreach (var cond in node.Conditions) {
                var (success, result) = Visit(cond);

                if (result is not bool itsTrue) {  // @note ESTE CHEKEO NO HAC FALTA XQ LA SINTAXIS ASEGURA Q ESO SEA BOOLEANO. PERO CUAN2 SOPORTEMOS LLAMADOS D FUNC EN UNA COND HAY Q HACERLO OBLIGAO
                    _log.LogError(
                        "Line {line}, column {col}: the condition #{idx} can't be a non-boolean expression.", 
                        node.Token.Line,
                        node.Token.Column,
                        idx+1);
                    return (false, null);
                }
                if (!success) {
                    return (false, null);
                }
                if (itsTrue) {
                    break;
                }
                idx++;
            }
            if (idx < node.Thens.Count) {  // hay un bloke q ejecutar. Se tiene en cuenta el bloke del else tambie'n
                Visit(node.Thens[idx]);
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

        public (bool, object) Visiting(SimpleW simpleW) {
            var ss = new SimpleServer(Context.Simulation, null);
            Context.Simulation.AddAgent(ss);
            return (true, ss);
        }

        public (bool, object) Visiting(GosListAst node) {
            var first = node.Elements.First();
            var (succ, val) = Visit(first);

            if (!succ) {
                return (false, null);
            }
            var fstType = Helper.GetType(val);  // el 1er tipo

            var ans = new List<object>();
            ans.Add(val);

            foreach (var elem in node.Elements.Skip(1)) {
                (succ, val) = Visit(elem);

                if (!succ) {
                    return (false, null);
                }
                var type = Helper.GetType(val);

                if (type != fstType) {  // todos los tipos deben ser iguales al 1ro
                    _log.LogError(
                        "Line {l}, column {c}: all elements in list must have the same type. Expected: {fstType}, actual: {type}.",
                        elem.Token.Line,
                        elem.Token.Column,
                        fstType,
                        type);
                    return (false, null);
                }
                ans.Add(val);
            }
            return (true, ans);
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