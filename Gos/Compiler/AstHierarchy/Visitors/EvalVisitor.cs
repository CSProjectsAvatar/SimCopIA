
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Agents;
using Compiler;
using Compiler.AstHierarchy;
using Compiler.AstHierarchy.Operands;
using Compiler.AstHierarchy.Operands.BooleanOperands;
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
        private bool _breakFlag;

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

        public (bool, object) Visiting(BoolAst node) {
            return (true, bool.Parse(node.Value));
        }

        public (bool, object) Visiting(ConjtionAst node) {
            var (succ, res) = Visit(node.Left);
            if (!succ) {
                return (false, null);
            }
            var type = Helper.GetType(res);
            if (type != GosType.Bool) {
                _log.LogError(
                    "Line {l}, column {c}: left operand must be a Bool but it's {type} instead.",
                    node.Token.Line,
                    node.Token.Column,
                    type);
                return default;
            }
            if (!(bool)res) {  // corto circuito
                return (true, false);
            }
            (succ, res) = Visit(node.Right);
            if (!succ) {
                return (false, null);
            }
            type = Helper.GetType(res);
            if (type != GosType.Bool) {
                _log.LogError(
                    "Line {l}, column {c}: right operand must be a Bool but its type is {type} instead.",
                    node.Token.Line,
                    node.Token.Column,
                    type);
                return default;
            }
            return (true, (bool)res);
        }

        public (bool, object) Visiting(DisjAst node) {
            var (succ, res) = Visit(node.Left);
            if (!succ) {
                return (false, null);
            }
            var type = Helper.GetType(res);
            if (type != GosType.Bool) {
                _log.LogError(
                    "Line {l}, column {c}: left operand must be a Bool but it's {type} instead.",
                    node.Token.Line,
                    node.Token.Column,
                    type);
                return default;
            }
            if ((bool)res) {  // corto circuito
                return (true, true);
            }
            (succ, res) = Visit(node.Right);
            if (!succ) {
                return (false, null);
            }
            type = Helper.GetType(res);
            if (type != GosType.Bool) {
                _log.LogError(
                    "Line {l}, column {c}: right operand must be a Bool but its type is {type} instead.",
                    node.Token.Line,
                    node.Token.Column,
                    type);
                return default;
            }
            return (true, (bool)res);
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
            var (succ, result) = VisitConnection(node, left, rightList);

            if(!succ){
                return (false, null);
            }

            return (true, null);
        }

        private (bool, object) VisitConnection(Connection node, Agent left, List<Agent> rightList) {
            return ((dynamic)this).Visiting(node, left, rightList);
        }

        public (bool, object) Visiting(RightConn node, Agent left, List<Agent> rightList) {
            throw new NotImplementedException();
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

        public (bool, object) Visiting(MethodCallAst node) {
            var (tsucc, tval) = Visit(node.Target);
            if (!tsucc) {
                return default;
            }
            var exprValues = new List<object>();
            foreach (var expr in node.Function.Args) {
                var (success, result) = Visit(expr);  // evaluando los argumentos
                if (!success) {
                    return (false, null);
                }
                exprValues.Add(result);
            }
            var ttype = Helper.GetType(tval);
            switch (ttype) {
                case GosType.List when node.Function.Identifier == "add" && node.Function.Args.Count == 1:
                    if (!TryEval(node.Function.Args, out IReadOnlyList<object> addArgs)) {
                        return default;
                    }
                    var l = tval as List<object>;

                    if (l.Count > 0) {
                        var expectedType = Helper.GetType(l[0]);
                        var itemType = Helper.GetType(addArgs[0]);
                        if (expectedType != itemType) {
                            _log.LogError(
                                Helper.LogPref + "expected type: {exp}, actual type: {type}.",
                                node.Function.Args[0].Token.Line,
                                node.Function.Args[0].Token.Column,
                                expectedType,
                                itemType);
                            return default;
                        }
                    }
                    l.Add(addArgs[0]);
                    break;

                case GosType.List when node.Function.Identifier == "del_at" && node.Function.Args.Count == 1:
                    if (!TryEval(node.Function.Args, out IReadOnlyList<object> delAtArgs)) {
                        return default;
                    }
                    l = tval as List<object>;

                    if (!IdxValid(delAtArgs[0], l.Count, node.Function.Args[0].Token.Line, node.Function.Args[0].Token.Column)) {
                        return default;
                    }
                    l.RemoveAt((int)(double)delAtArgs[0] - 1);
                    break;

                default:
                    _log.LogError(
                        Helper.LogPref + "no method called '{method}' in type {type} receives {args} arguments.",
                        node.Function.Token.Line,
                        node.Function.Token.Column,
                        node.Function.Identifier,
                        ttype,
                        node.Function.Args.Count);
                    return default;
            }
            return (true, null);
        }

        private bool TryEval(IEnumerable<Expression> exprs, out IReadOnlyList<object> results) {
            results = null;

            var ans = new List<object>();
            foreach (var expr in exprs) {
                var (success, result) = Visit(expr);
                if (!success) {
                    return false;
                }
                ans.Add(result);
            }
            results = ans;
            return true;
        }

        public (bool, object) Visiting(LetVar node){
            var (success, result) = Visit(node.Expr);
            if(!success){
                return (false, null);
            }
            if (result is Agent serv) {
                serv.ID = node.Identifier;
            }
            Context.DefVariable(node.Identifier, result);
            return (true, result);  
        }

        public (bool, object) Visiting(AssignAst node) {
            return node.Left switch {
                Variable v => Visiting(new VarAssign(Helper.Logger<VarAssign>()) {
                    Token = node.Token,
                    Variable = v.Identifier,
                    NewValueExpr = node.NewVal
                }),
                ListIdxGetAst lg => Visiting(new ListIdxSetAst(Helper.Logger<VarAssign>()) {
                    NewValueExpr = node.NewVal,
                    Idx = lg.Index,
                    Target = lg.Left,
                    Token = node.Token
                }),
                PropGetAst pg => Visiting(new PropSetAst {
                    Token = node.Token,
                    Target = pg.Target,
                    Property = pg.Property,
                    NewVal = node.NewVal
                }),
                _ => throw new ArgumentException("Invalid left value.", nameof(node))
            };
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
            var (tsucc, tvalue) = Visit(node.Target);
            if (!tsucc) {
                return (false, null);
            }
            var ttype = Helper.GetType(tvalue);
            if (ttype != GosType.List) {
                _log.LogError(
                    Helper.LogPref + "target object must be a list, but it's of type {ttype} instead.",
                    node.Token.Line,
                    node.Token.Column,
                    ttype);
                return default;
            }
            var (idxSucc, idxVal) = Visit(node.Idx);
            var list = tvalue as List<object>;
            if (!idxSucc || !IdxValid(idxVal, list.Count, node.Idx.Token.Line, node.Idx.Token.Column)) {
                return default;
            }
            var (vsucc, value) = Visit(node.NewValueExpr);
            if (!vsucc) {
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
            list[(int)(double)idxVal - 1] = value;

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
                    "Line {l}, column {c}: index must be greater than 0 and less or equal than the list size ({lsize}).",
                    line,
                    column,
                    listCount);
                return false;
            }
            return true;
        }

        public (bool, object) Visiting(PropSetAst node) {
            var (tsucc, tval) = Visit(node.Target);
            if (!tsucc) {
                return default;
            }
            var type = Helper.GetType(tval);
            switch (type) {
                default:
                    _log.LogError(
                        Helper.LogPref + "{type} doesn't have a property called '{prop}' or it's value can't be set.",
                        node.Token.Line,
                        node.Token.Column,
                        type,
                        node.Property);
                    return default;
            }
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

        public (bool, object) Visiting(PropGetAst node) {
            var (tsucc, tval) = Visit(node.Target);
            if (!tsucc) {
                return default;
            }
            var type = Helper.GetType(tval);
            switch (type) {
                case GosType.List when node.Property == "length":
                    return (true, (double)(tval as List<object>).Count);
                default:
                    _log.LogError(
                        Helper.LogPref + "{type} doesn't have a property called '{prop}'.",
                        node.Token.Line,
                        node.Token.Column,
                        type,
                        node.Property);
                    return default;
            }
        }

        public (bool, object) Visiting(DefFun node){
            Context.SetFunc(node.Identifier, node.Arguments.Count, node);
            return (true, null);  
        }

        public (bool, object) Visiting(BehaviorAst node) {
            Context.DefBehav(node.Name, node);
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
            
            //if (vis.Item1) {  // evaluacio'n exitosa
            //    // @audit DE JUGUETE
            //    Context.Simulation.AddSomeRequests();
            //    Context.Simulation.Run();

            //    _log.LogInformation(
            //        Context.Simulation.solutionResponses.Aggregate(
            //            $"Responses to environment:{System.Environment.NewLine}",
            //            (accum, r) => accum + $"time:{r.responseTime} body:{r.body}{System.Environment.NewLine}"));
            //}
            return vis;
        }

        public (bool, object) Visiting(InfLoopAst node) {
            return (
                Loop(default, default, InfLoop(), node.Statements), 
                null);
        }

        public (bool, object) Visiting(ForEachAst node) {
            var (iterSucc, iterable) = Visit(node.Iterable);

            if (!iterSucc) {
                return (false, null);
            }
            var type = Helper.GetType(iterable);
            if (type != GosType.List) {
                _log.LogError(
                    "Line {l}, column {c}: for statement must iterate over a list, but {type} was given.",
                    node.Token.Line,
                    node.Token.Column,
                    type);
                return (false, null);
            }
            return (
                Loop(node.Index, node.Item, iterable as List<object>, node.Code),
                null);
        }

        /// <summary>
        /// Performs a loop over <paramref name="target"/> executing <paramref name="code"/>. The actual item is assigned to
        /// a variable of name <paramref name="itemName"/> and the number of the actual iteration is assigned to a variable of 
        /// name
        /// <paramref name="idxName"/>.
        /// </summary>
        /// <param name="idxName">Name of the index variable.</param>
        /// <param name="itemName">Name of the item variable.</param>
        /// <param name="target"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        private bool Loop(string idxName, string itemName, IEnumerable<object> target, IEnumerable<IStatement> code) {
            double i = 1;  // es uno porque las listas indexan en base-1
            
            foreach (var item in target) {
                Context = Context.CreateChildContext();

                if (idxName != default) {
                    Context.DefVariable(idxName, i++);
                }
                if (itemName != default) {
                    Context.DefVariable(itemName, item);
                }
                var succ = Visit(code).Item1;
                stackC.Pop();

                if (!succ) {
                    return false;
                }
                if (_breakFlag) {
                    _breakFlag = false;
                    break;
                }
            }
            return true;
        }

        public (bool, object) Visiting(BreakAst _) {
            _breakFlag = true;
            return (true, null);
        }

        /// <summary>
        /// Infinite loop.
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<object> InfLoop() {
            while (true) {
                yield return 1;
            }
        }

        public (bool, object) Visiting(IfStmt node){
            int idx = 0;  // el i'ndic de la 1ra cond q es true

            foreach (var cond in node.Conditions) {
                var (success, result) = Visit(cond);
                var type = Helper.GetType(result);

                if (type != GosType.Bool) {
                    _log.LogError(
                        "Line {line}, column {col}: Bool expression expected in condition #{idx} but {type} given instead.", 
                        node.Token.Line,
                        node.Token.Column,
                        idx+1,
                        type);
                    return (false, null);
                }
                if (!success) {
                    return (false, null);
                }
                if ((bool)result) {
                    break;
                }
                idx++;
            }
            if (idx < node.Thens.Count) {  // hay un bloke q ejecutar. Se tiene en cuenta el bloke del else tambie'n
                Context = Context.CreateChildContext(); // cambiando el contexto

                var succ = Visit(node.Thens[idx]).Item1;

                stackC.Pop(); // retornan2 al contexto previo

                if (!succ) {  // evaluacio'n no exitosa
                    return (false, null);
                }
            }
            return (true, null); 
        }
        

        /// <summary>
        /// Evalua una lista de statements y devuelve el resultado de la ultima
        /// </summary>
       public (bool, object) Visiting(Return node){
            object result = null;

            if (node.Expr != null) {
                bool success;
                (success, result) = Visit(node.Expr);
                if(!success){
                    return (false, null);
                }
            }
            _returnFlag = (true, result);
            return (true, result);    
        }
        
        public (bool, object) Visiting(ClassAst node){
            throw new NotImplementedException();
        }

        public (bool, object) Visiting(GosListAst node) {
            var ans = new List<object>();

            if (!node.Elements.Any()) {
                return (true, ans);
            }
            var first = node.Elements.First();
            var (succ, val) = Visit(first);

            if (!succ) {
                return (false, null);
            }
            var fstType = Helper.GetType(val);  // el 1er tipo

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

        public (bool, object) Visiting(IEnumerable<IStatement> statements){  // @remind debe ser llama2 100pre q c kiera ejecutar un bloke d co'digo
            object lastR = null;

            foreach (var st in statements){
                var (success, result) = Visit(st);
                if(!success){
                    return (false, null);
                }
                lastR = result;

                if(_returnFlag.Found || _breakFlag) 
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