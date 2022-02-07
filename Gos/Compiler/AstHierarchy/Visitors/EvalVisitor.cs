
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compiler;
using Compiler.AstHierarchy;
using Compiler.AstHierarchy.Operands;
using Compiler.AstHierarchy.Operands.BooleanOperands;
using Compiler.AstHierarchy.Statements;
using Core;
using Microsoft.Extensions.Logging;
using ServersWithLayers;
using ServersWithLayers.Behaviors;

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
            throw new EntryPointNotFoundException("Se llego por el visitor a la raiz, falta implementacio'n de Visiting");
        }  
    
         public (bool, object) Visiting(BinaryExpr node) {  
            if(!CheckBinar(node, out var left, out var right)) {
                return (false, null);
            }
            var tResults = (left, right);

            switch (tResults) {  // @audit SEGURAMENT NECESITAREMOS UN DICCIONARIO PA SABER Q TIPOS ADMITE CADA OPERADOR EN SUS OPERANDOS
                case (_, _) when node is EqEqOp eqeq:
                    var (succ, result) = Visiting(eqeq, left, right);
                    if (!succ) {
                        return (false, null);
                    }
                    return (true, result);

                case (double lNum, double rNum):
                    (succ, result) = VisitBinExpr(node, lNum, rNum);
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

        public (bool Success, object Result) Visiting(EqEqOp node, object lobj, object robj) {
            return (true, Equals(lobj, robj));
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

        public (bool Success, object Result) Visiting(IsTypeAst node) {
            var (tsucc, tval) = Visit(node.Target);
            if (!tsucc) {
                return default;
            }
            var ttype = Helper.GetType(tval);
            if (!Enum.TryParse<GosType>($"{char.ToUpper(node.Type[0])}{node.Type[1..]}", out var isType)) {
                throw new NotImplementedException();
            }
            if (node.NewVar != default) {
                Context.DefVariable(node.NewVar, tval);
            }
            return (true, (ttype == isType) ^ node.Not);
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

        //public (bool, object) Visiting(Connection node) {  
        //    var eval = CheckConnection(node, out var left, out var rightList);
        //    if(!eval){
        //        return (false, null);
        //    }
        //    var (succ, result) = VisitConnection(node, left, rightList);

        //    if(!succ){
        //        return (false, null);
        //    }

        //    return (true, null);
        //}

        //private (bool, object) VisitConnection(Connection node, Agent left, List<Agent> rightList) {
        //    return ((dynamic)this).Visiting(node, left, rightList);
        //}

        //public (bool, object) Visiting(RightConn node, Agent left, List<Agent> rightList) {
        //    throw new NotImplementedException();
        //}

        //private bool CheckConnection(Connection node, out Agent left, out List<Agent> agList)
        //{
        //    left = null;
        //    agList = new List<Agent>();
        //    // Checking Types of Agents
        //    foreach (var serv in node.Agents.Concat(new[] { node.LeftAgent }))
        //    {
        //        var varInstance = Context.GetVar(serv);
        //        var varType = Helper.GetType(varInstance);

        //        if (varType is not GosType.Server) {
        //            _log?.LogError(
        //                "Line {line}, column {col}: variable '{serv}' has to be of type Server but it's of type {type}.",
        //                node.Token.Line,
        //                node.Token.Column,
        //                serv,
        //                varType);
        //            return false;
        //        }
        //        agList.Add(varInstance as Agent);
        //    }
        //    left = agList.Last();
        //    agList.RemoveAt(agList.Count - 1);
        //    return true;
        //}

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

            if (defFun == null) {  // funcio'n built-in sin co'digo en GoS. Debe ser implementada en C#
                return BuiltinCall(node.Identifier, exprValues, node.Token);
            }
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

        private (bool, object) BuiltinCall(string funName, IReadOnlyList<object> args, Token token) {
            switch (funName) {
                case Helper.RandFun:
                    if (!IsOfTypeWithLog(GosType.Number, args[0], token)) {
                        return default;
                    }
                    return (true, Helper.Random.NextDouble() * (double)args[0]);

                case Helper.RandIntFun:
                    if (!IsOfTypeWithLog(GosType.Number, args[0], token)) {
                        return default;
                    }
                    return (true, (double)(int)(Helper.Random.NextDouble() * (double)args[0]));

                default:
                    throw new NotImplementedException("Not implemented built-in call.");
            }
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
                #region lista
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
                #endregion

                case GosType.ServerStatus when node.Function.Identifier == "missing_resrcs" && node.Function.Args.Count == 1:
                    if (!TryEval(node.Function.Args[0], GosType.Request, out var req)) {
                        return default;
                    }
                    return (
                        true,
                        BossBehav.FilterNotAvailableRscs(tval as Status, (req as Request).AskingRscs)
                            .OfType<object>()
                            .ToList());
                case GosType.ServerStatus when node.Function.Identifier == "reward" && node.Function.Args.Count == 2:
                    if (!TryEval(node.Function.Args[0], GosType.Response, out var resp)
                            || !TryEval(node.Function.Args[1], GosType.Number, out var delay)) {
                        return default;
                    }
                    if (!Helper.IsInteger((double)delay)) {
                        _log.LogError(
                            Helper.LogPref + "second parameter can't be a fractional number.",
                            node.Function.Args[1].Token.Line,
                            node.Function.Args[1].Token.Column
                            );
                        return default;
                    }
                    var status = tval as Status;
                    status.Reward(resp as Response, (int)(double)delay);

                    return (true, null);
                case GosType.ServerStatus when node.Function.Identifier == "reqs_for_best" && node.Function.Args.Count == 1:
                    if (!TryEval(node.Function.Args[0], GosType.List, out var respsObj)) {
                        return default;
                    }
                    var resps = respsObj as List<object>;
                    if (resps.Count == 0) {
                        _log.LogWarning(
                            Helper.LogPref + "empty list; no requests will be given.",
                            node.Function.Args[0].Token.Line,
                            node.Function.Args[0].Token.Column);
                    }
                    return (
                        true,
                        BossBehav.ResponseSelectionFunction(tval as Status, resps.OfType<Response>())
                            .OfType<object>()
                            .ToList());
                case GosType.ServerStatus when node.Function.Identifier == "providers" && node.Function.Args.Count == 1:
                    if (!TryEval(node.Function.Args[0], GosType.Resource, out var resrcObj)) {
                        return default;
                    }
                    return (
                        true,
                        (tval as Status).Providers(resrcObj as Resource)
                            .OfType<object>()
                            .ToList());

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
            //if (result is Agent serv) {
            //    serv.ID = node.Identifier;
            //}
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

        /// <summary>
        /// Visita <paramref name="node"/> y se asegura de que el tipo del resultado obtenido sea <paramref name="expected"/>.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="expected"></param>
        /// <param name="newVal">El resultado obtenido de la evaluacio'n.</param>
        /// <returns>Si pudo ejecutar y el resultado obtenido es del tipo esperado.</returns>
        private bool TryEval(AstNode node, GosType expected, out object newVal) {
            bool vsucc;
            (vsucc, newVal) = Visit(node);
            if (!vsucc) {
                return false;
            }
            return IsOfTypeWithLog(expected, newVal, node.Token);
        }

        private bool IsOfTypeWithLog(GosType expected, object val, Token token) {
            var type = Helper.GetType(val);
            if (type != expected) {
                _log.LogError(
                    Helper.LogPref + "expected type: {expect}; actual type: {type}.",
                    token.Line,
                    token.Column,
                    expected,
                    type);
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
                case GosType.ServerStatus when node.Property == "leader":
                    if (!TryEval(node.NewVal, GosType.String, out var newVal)) {
                        return default;
                    }
                    (tval as Status).ChangeLeader(newVal as string);
                    break;

                case GosType.Resource when node.Property == "time":
                    if (!TryEval(node.NewVal, GosType.Number, out var timeObj)) {
                        return default;
                    }
                    var timeDouble = (double)timeObj;
                    if (!Helper.IsInteger(timeDouble)) {
                        _log.LogError(
                            Helper.LogPref + "resource time can't be a fractional number.",
                            node.NewVal.Token.Line,
                            node.NewVal.Token.Column
                            );
                        return default;
                    }
                    (tval as Resource).SetTime((int)timeDouble);
                    break;
                case GosType.Resource when node.Property == "requirements":
                    if (!TryEval(node.NewVal, GosType.List, out var listObj)) {
                        return default;
                    }
                    var list = listObj as List<object>;
                    if (!LogIfEmpty(list, node.NewVal.Token)) {
                        if (!IsOfTypeWithLog(GosType.Resource, list[0], node.NewVal.Token)) {
                            return default;
                        }
                    }
                    (tval as Resource).AddReqList(list.OfType<Resource>().ToList());

                    break;

                case GosType.Layer when node.Property == "behaviors":
                    if (!TryEval(node.NewVal, GosType.List, out listObj)) {
                        return default;
                    }
                    list = listObj as List<object>;
                    if (!LogIfEmpty(list, node.NewVal.Token)) {
                        if (!IsOfTypeWithLog(GosType.Behavior, list[0], node.NewVal.Token)) {
                            return default;
                        }
                    }
                    (tval as Layer).SetBehaviors(list.OfType<Behavior>());

                    break;
                case GosType.Layer when node.Property == "selector":
                    if (!TryEval(node.NewVal, GosType.Function, out var selectorObj)) {
                        return default;
                    }
                    var selector = selectorObj as DefFun;
                    if (selector.Arguments.Count != 1) {
                        _log.LogError(
                            Helper.LogPref + "selector function must receive only one argument.",
                            node.NewVal.Token.Line,
                            node.NewVal.Token.Column
                            );
                        return default;
                    }
                    var layer = tval as Layer;
                    layer.SetBehaviourSelector(bs => {
                        var (succ, res) = Visiting(new FunCall {
                            Token = node.NewVal.Token,
                            Args = new() {
                                new GosListAst { 
                                    Elements = layer.Behavs
                                        .Select(b => new Variable {
                                            Identifier = b.Name
                                        })
                                }
                            },
                            Identifier = selector.Identifier
                        });
                        if (!succ) {
                            throw new GoSException($"Runtime error in selector function {selector.Identifier}.");
                        }
                        if (!IsOfTypeWithLog(GosType.Number, res, node.NewVal.Token)) {
                            _log.LogError(
                                Helper.LogPref + "returned value must be a number.",
                                node.NewVal.Token.Line,
                                node.NewVal.Token.Column);
                            throw new GoSException($"Returned value must be a number.");
                        }
                        return (int)(double)res - 1;
                    });

                    break;

                case GosType.Server when node.Property == "layers":
                    if (!TryEval(node.NewVal, GosType.List, out listObj)) {
                        return default;
                    }
                    list = listObj as List<object>;
                    if (!LogIfEmpty(list, node.NewVal.Token)) {
                        if (!IsOfTypeWithLog(GosType.Layer, list[0], node.NewVal.Token)) {
                            return default;
                        }
                    }
                    (tval as Server).SetLayers(list.OfType<Layer>());

                    break;
                case GosType.Server when node.Property == "resources":
                    if (!TryEval(node.NewVal, GosType.List, out listObj)) {
                        return default;
                    }
                    list = listObj as List<object>;
                    if (!LogIfEmpty(list, node.NewVal.Token)) {
                        if (!IsOfTypeWithLog(GosType.Resource, list[0], node.NewVal.Token)) {
                            return default;
                        }
                    }
                    (tval as Server).SetResources(list.OfType<Resource>());

                    break;

                default:
                    _log.LogError(
                        Helper.LogPref + "{type} doesn't have a property called '{prop}' or it's value can't be set.",
                        node.Token.Line,
                        node.Token.Column,
                        type,
                        node.Property);
                    return default;
            }
            return (true, null);
        }

        /// <summary>
        /// Notifica si la lista esta' vaci'a.
        /// </summary>
        /// <param name="list"></param>
        /// <returns>Si la lista esta' vaci'a.</returns>
        private bool LogIfEmpty(List<object> list, Token token) {
            if (list.Count == 0) {
                _log.LogWarning(
                    Helper.LogPref + "empty list.",
                    token.Line,
                    token.Column);
                return true;
            }
            return false;
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

                case GosType.ServerStatus when node.Property == "accepted_reqs":
                    return (true, (tval as Status).AceptedRequests.OfType<object>().ToList());
                case GosType.ServerStatus when node.Property == "can_process":
                    var status = tval as Status;
                    return (true, status.HasCapacity);
                case GosType.ServerStatus when node.Property == "leader":
                    return (true, (tval as Status).LeaderId());
                case GosType.ServerStatus when node.Property == "my_server":
                    return (true, (tval as Status).ServerId);
                case GosType.ServerStatus when node.Property == "neighbors":
                    return (
                        true, 
                        (tval as Status)
                            .GetNeighbors()
                            .OfType<object>()
                            .ToList());

                case GosType.Request when node.Property == "type":
                    return (true, (double)(tval as Request).Type);
                case GosType.Request when node.Property == "resources":
                    return (
                        true, 
                        (tval as Request)
                            .AskingRscs
                            .OfType<object>()
                            .ToList());

                case GosType.Response when node.Property == "req_type":
                    return (true, (double)(tval as Response).Type);
                case GosType.Response when node.Property == "resources":
                    return (
                        true,
                        (tval as Response)
                            .AnswerRscs
                            .Where(kv => kv.Value)
                            .Select(kv => Resource.Rsrc(kv.Key))
                            .OfType<object>()
                            .ToList());
                case GosType.Response when node.Property == "req_id":
                    return (true, (double)(tval as Response).ReqID);

                #region comunes a pedi2 y respuesta
                case GosType.Request or GosType.Response when node.Property == "sender":
                    return (true, (tval as Message).Sender);
                case GosType.Request or GosType.Response when node.Property == "id":
                    return (true, (double)(tval as Message).ID);
                #endregion

                case GosType.Resource when node.Property == "name":
                    return (true, (tval as Resource).Name);
                case GosType.Resource when node.Property == "time":
                    return (true, (double)(tval as Resource).RequiredTime);
                case GosType.Resource when node.Property == "required":
                    return (true, (tval as Resource).IsRequired);
                case GosType.Resource when node.Property == "requirements":
                    return (
                        true, 
                        (tval as Resource)
                            .GetRequirements()
                            .OfType<object>()
                            .ToList());

                case GosType.Server when node.Property == "id":
                    return (true, (tval as Server).ID);

                case GosType.Environment when node.Property == "time":
                    return (true, (double)Env.Time);

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
            Context.DefVariable(node.Identifier, node);
            return (true, null);  
        }

        public (bool, object) Visiting(BehaviorAst node) {
            // func principal del comportamiento
            void Main(Status status, Perception percep, Dictionary<string, object> vars) {
                Context = Context.CreateChildContext();
                // definien2 variables
                Context.DefVariable(Helper.StatusVar, status);
                Context.DefVariable(Helper.PercepVar, percep);
                vars
                    .ToList()
                    .ForEach(kv => Context.DefVariable(kv.Key, kv.Value));  // las definiciones d otras vars ma'giks deben ir a partir d este punto
                DefDoneReqs();
                Context.DefVariable(Helper.EnvVar, Env.CurrentEnv);

                var (succ, _) = Visiting(node.Code.Where(st => st is not InitAst));  // ejecutan2 co'digo principal (obvian2 bloke init)
                _returnFlag = default;

                if (!succ) {
                    stackC.Pop();
                    throw new GoSException($"Runtime error in behavior '{node.Name}' main code.");
                }
                vars
                    .ToList()
                    .ForEach(kv => vars[kv.Key] = Context.GetVar(kv.Key));  // actualizan2 variables en el diccionario

                stackC.Pop();
            }
            // init del comportamiento
            void Init(Status status, Dictionary<string, object> vars) {
                Context = Context.CreateChildContext();
                Context.DefVariable(Helper.HiddenDoneReqsHeapVar, new Utils.Heap<Request>());  // variable oculta, el usuario no debe emplearla

                var init = node.Code
                    .OfType<InitAst>()
                    .FirstOrDefault();
                if (init != default) {  // hay un bloke init
                    Context.DefVariable(Helper.StatusVar, status);
                    Context.DefVariable(Helper.EnvVar, Env.CurrentEnv);

                    var (succ, _) = Visit(init);  // ejecutan2 bloke init

                    if (!succ) {
                        stackC.Pop();
                        throw new GoSException($"Runtime error in behavior '{node.Name}' init code.");
                    }
                }
                Context.Variables
                    .Where(v => v.Name != Helper.StatusVar)
                    .ToList()
                    .ForEach(v => vars[v.Name] = v.Value);  // anyadien2 variables al diccionario

                stackC.Pop();
            }

            var behav = new Behavior(Main, Init) {
                Name = node.Name
            };

            Context.DefBehav(
                node.Name,
                behav);
            Context.DefVariable(node.Name, behav);

            return (true, null);
        }

        /// <summary>
        /// Define la lista de pedidos q ya fueron procesa2.
        /// </summary>
        private void DefDoneReqs() {
            var heap = Context.GetVar(Helper.HiddenDoneReqsHeapVar) as Utils.Heap<Request>;
            var ans = new List<object>();
            var extracted = new List<(int, Request)>();

            while (heap.Count != 0 && heap.First.Item1 <= Env.Time) {
                var elem = heap.RemoveMin();

                ans.Add(elem.Item2);
                extracted.Add(elem);
            }
            extracted.ForEach(elem => heap.Add(elem.Item1, elem.Item2));  // reinsertan2, por si alguien kiere volver a usar el heap

            Context.DefVariable(Helper.DoneReqsVar, ans);
        }

        public (bool, object) Visiting(InitAst node) {
            foreach (var assign in node.Code.OfType<AssignAst>()) {
                var (succ, _) = Visit(new LetVar {
                    Expr = assign.NewVal,
                    Identifier = (assign.Left as Variable).Identifier,
                    Token = assign.Token
                });
                if (!succ) {
                    return default;
                }
            }
            return (true, null);
        }

        public (bool, object) Visiting(RespondOrSaveAst node) {
            var (rsucc, rval) = Visit(node.Request);
            if (!rsucc) {
                return default;
            }
            var rtype = Helper.GetType(rval);
            if (rtype != GosType.Request) {
                _log.LogError(
                    Helper.LogPref + "expected type: Request, actual type: {type}.",
                    node.Request.Token.Line,
                    node.Request.Token.Column,
                    rtype);
                return default;
            }
            var req = rval as Request;
            var status = Context.GetVar(Helper.StatusVar) as Status;
            if (!TryRemoveFromProcessing(req, node.Request.Token, status)) {
                return default;
            }
            Response response = BehaviorsLib.BuildResponse(status, req);

            if (BehaviorsLib.Incomplete(status, response)) {
                _log.LogInformation("Incomplete {r}", GosObjToString(response));

                status.AddPartialRpnse(response);
            } else {
                status.Subscribe(response);
            }
            return (true, null);
        }

        /// <summary>
        /// Retorna si lo pudo eliminar del heap de pedi2 en procesamiento.
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        private bool TryRemoveFromProcessing(Request req, Token reqToken, Status status) {
            if (!status.DecProcessing()) {
                _log.LogError(
                    Helper.LogPref + "there aren't requests in the processing state.",
                    reqToken.Line,
                    reqToken.Column
                    );
                return false;
            }
            var heap = Context.GetVar(Helper.HiddenDoneReqsHeapVar) as Utils.Heap<Request>;
            var toReturn = new List<(int, Request)>();
            var found = false;
            for ((int, Request) pair; heap.Count != 0; ) {
                pair = heap.RemoveMin();

                if (pair.Item2.ID == req.ID) {
                    found = true;
                    break;
                }
                toReturn.Add(pair);
            }
            toReturn.ForEach(elem => heap.Add(elem.Item1, elem.Item2));  // reinsertan2 los pedid2 !=s a req

            if (!found) {
                status.IncProcessing();
                _log.LogError(
                    Helper.LogPref + "only a request in the processing state can be removed.",
                    reqToken.Line,
                    reqToken.Column
                    );
                return false;
            }
            return true;
        }

        public (bool, object) Visiting(ProcessAst node) {
            var (rsucc, rval) = Visit(node.Request);
            if (!rsucc) {
                return default;
            }
            var rtype = Helper.GetType(rval);
            if (rtype != GosType.Request) {
                _log.LogError(
                    Helper.LogPref + "expected type: Request, actual type: {type}.",
                    node.Request.Token.Line,
                    node.Request.Token.Column,
                    rtype);
                return default;
            }
            var req = rval as Request;
            var st = Context.GetVar(Helper.StatusVar) as Status;

            if (!st.IncProcessing()) {
                _log.LogError(
                    Helper.LogPref + "server capacity exceeded, no more requests can be processed.",
                    node.Token.Line,
                    node.Token.Column);
                return default;
            }
            st.EnsureExtractedFromAccepted(req);

            int rtime = BehaviorsLib.GetRequiredTimeToProcess(req);
            var heap = Context.GetVar(Helper.HiddenDoneReqsHeapVar) as Utils.Heap<Request>;

            heap.Add(Env.Time + rtime, req);  // comienzo a procesar la tarea
            st.SubscribeIn(rtime, new Observer(st.serverID));

            return (true, null);
        }

        public (bool, object) Visiting(RespondAst node) {
            var (rsucc, rval) = Visit(node.Request);
            if (!rsucc) {
                return default;
            }
            var rtype = Helper.GetType(rval);
            if (rtype != GosType.Request) {
                _log.LogError(
                    Helper.LogPref + "expected type: Request, actual type: {type}.",
                    node.Request.Token.Line,
                    node.Request.Token.Column,
                    rtype);
                return default;
            }
            var req = rval as Request;
            var status = Context.GetVar(Helper.StatusVar) as Status;

            Response response = BehaviorsLib.BuildResponse(status, req);

            _log.LogInformation("Subscribing {r}", GosObjToString(response));
            status.Subscribe(response);

            return (true, null);
        }

        public (bool, object) Visiting(AcceptAst node) {
            var (rsucc, rval) = Visit(node.Request);
            if (!rsucc) {
                return default;
            }
            var rtype = Helper.GetType(rval);
            if (rtype != GosType.Request) {
                _log.LogError(
                    Helper.LogPref + "expected type: Request, actual type: {type}.",
                    node.Request.Token.Line,
                    node.Request.Token.Column,
                    rtype);
                return default;
            }
            var req = rval as Request;
            if (req.Type is not ReqType.DoIt) {
                _log.LogError(
                    Helper.LogPref + "only DO requests are accepted for processing later.",
                    node.Request.Token.Line,
                    node.Request.Token.Column);
                return default;
            }
            var status = Context.GetVar(Helper.StatusVar) as Status;

            _log.LogInformation("Accepting {r}", GosObjToString(req));
            status.AcceptReq(req);

            return (true, null);
        }

        public (bool, object) Visiting(PingAst node) {
            if (!TryEval(node.Target, GosType.String, out var target)) {
                return default;
            }
            var toArrival = 0;
            if (node.AfterNow != null && TryEval(node.AfterNow, GosType.Number, out var afterNow)) {
                var afterNowDouble = (double)afterNow;
                if (!Helper.IsInteger(afterNowDouble)) {
                    _log.LogError(
                        Helper.LogPref + "time until PING arrival can't be a fractional number.",
                        node.AfterNow.Token.Line,
                        node.AfterNow.Token.Column);
                    return default;
                }
                toArrival = (int)afterNowDouble;

            } else if (node.AfterNow != null) {  // error en la evaluacio'n del afterNow
                return default;
            }
            var targetId = target as string;
            var status = Context.GetVar(Helper.StatusVar) as Status;
            var ping = new Request(status.ServerId, targetId, ReqType.Ping);

            if (toArrival == 0) {
                status.Subscribe(ping);
            } else {
                status.SubscribeIn(toArrival, ping);
            }
            return (true, ping);
        }

        public (bool, object) Visiting(AlarmMeAst node) {
            if (!TryEval(node.AfterNow, GosType.Number, out var afterNow)) {
                return default;
            }
            var afterNowDouble = (double)afterNow;
            if (!Helper.IsInteger(afterNowDouble)) {
                _log.LogError(
                    Helper.LogPref + "time until ALARM arrival can't be a fractional number.",
                    node.AfterNow.Token.Line,
                    node.AfterNow.Token.Column);
                return default;
            }
            var status = Context.GetVar(Helper.StatusVar) as Status;
            status.SubscribeIn((int)afterNowDouble, new Observer(status.ServerId));

            return (true, null);
        }

        public (bool, object) Visiting(AskAst node) {
            if (!TryEval(node.Target, GosType.String, out var target)) {
                return default;
            }
            var toArrival = 0;
            if (node.AfterNow != null && TryEval(node.AfterNow, GosType.Number, out var afterNow)) {
                var afterNowDouble = (double)afterNow;
                if (!Helper.IsInteger(afterNowDouble)) {
                    _log.LogError(
                        Helper.LogPref + "time until ASK arrival can't be a fractional number.",
                        node.AfterNow.Token.Line,
                        node.AfterNow.Token.Column);
                    return default;
                }
                toArrival = (int)afterNowDouble;

            } else if (node.AfterNow != null) {  // error en la evaluacio'n del afterNow
                return default;
            }
            if (!TryEval(node.Resources, GosType.List, out var resources)) {
                return default;
            }
            var rsrcs = resources as List<object>;
            if (rsrcs.Count == 0) {
                _log.LogWarning(
                    Helper.LogPref + "the list is empty, no resources are sent.",
                    node.Resources.Token.Line,
                    node.Resources.Token.Column);
            } else if (!IsOfTypeWithLog(GosType.Resource, rsrcs[0], node.Resources.Token)) {
                return default;
            }
            var targetId = target as string;
            var status = Context.GetVar(Helper.StatusVar) as Status;
            var ask = new Request(status.ServerId, targetId, ReqType.Asking);
            ask.AskingRscs.AddRange(rsrcs.OfType<Resource>());

            if (toArrival == 0) {
                status.Subscribe(ask);
            } else {
                status.SubscribeIn(toArrival, ask);
            }
            return (true, ask);
        }

        public (bool, object) Visiting(OrderAst node) {
            if (!TryEval(node.Target, GosType.String, out var target)) {
                return default;
            }
            var toArrival = 0;
            if (node.AfterNow != null && TryEval(node.AfterNow, GosType.Number, out var afterNow)) {
                var afterNowDouble = (double)afterNow;
                if (!Helper.IsInteger(afterNowDouble)) {
                    _log.LogError(
                        Helper.LogPref + "time until ORDER arrival can't be a fractional number.",
                        node.AfterNow.Token.Line,
                        node.AfterNow.Token.Column);
                    return default;
                }
                toArrival = (int)afterNowDouble;

            } else if (node.AfterNow != null) {  // error en la evaluacio'n del afterNow
                return default;
            }
            if (!TryEval(node.Resources, GosType.List, out var resources)) {
                return default;
            }
            var rsrcs = resources as List<object>;
            if (rsrcs.Count == 0) {
                _log.LogWarning(
                    Helper.LogPref + "the list is empty, no resources are sent.",
                    node.Resources.Token.Line,
                    node.Resources.Token.Column);
            } else if (!IsOfTypeWithLog(GosType.Resource, rsrcs[0], node.Resources.Token)) {
                return default;
            }
            var targetId = target as string;
            var status = Context.GetVar(Helper.StatusVar) as Status;
            var order = new Request(status.ServerId, targetId, ReqType.DoIt);
            order.AskingRscs.AddRange(rsrcs.OfType<Resource>());

            if (toArrival == 0) {
                status.Subscribe(order);
            } else {
                status.SubscribeIn(toArrival, order);
            }
            return (true, order);
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

        internal static string GosObjToString(object obj) {
            switch (Helper.GetType(obj)) {
                case GosType.List:
                    return $"[{string.Join(", ", (obj as List<object>).Select(elem => GosObjToString(elem)))}]";

                case GosType.Request:
                    var r = obj as Request;
                    var type = r.Type switch {
                        ReqType.Asking => "ASK",
                        ReqType.DoIt => "DO",
                        ReqType.Ping => "PING",
                        _ => throw new NotImplementedException()
                    };
                    var resrcs = string.Join(", ", r.AskingRscs.Select(rs => rs.Name));
                    return $"request {r.ID} {r.Sender} --{type}--> {r.Receiver} ({resrcs})";

                case GosType.Response:
                    var resp = obj as Response;
                    return $"response to request {resp.ReqID}";

                case GosType.Resource:
                    return $"resource {(obj as Resource).Name}";

                default:
                    return obj?.ToString();
            }
        }

        public (bool, object) Visiting(ProgramNode node){
            var vis = Visit(node.Statements);
            if (!vis.Item1) {
                return default;
            }
            var servs = AccessibleServers().ToList();
            if (servs.Count == 0) {
                _log.LogWarning("No servers are accessible from the global context. Simulation won't run.");
            } else {
                var env = new Env(Helper.Logger<Env>(), Helper.Logger<MicroService>());
                env.AddServerList(servs);

                try {
                    env.Run();
                } catch (GoSException e) {
                    _log.LogError(e.Message);
                    return default;
                }
            }
            return (true, null);
        }

        internal IEnumerable<Server> AccessibleServers() {
            var ans = Enumerable.Empty<Server>();

            foreach (var (_, val) in Context.Variables) {
                ans = ans.Concat(AccessibleServers(val));
            }
            return ans;
        }

        private IEnumerable<Server> AccessibleServers(object val) {
            switch (Helper.GetType(val)) {
                case GosType.Server:
                    return new[] { val as Server };

                case GosType.List:
                    var l = val as List<object>;
                    return l.Aggregate(Enumerable.Empty<Server>(), (accum, obj) => accum.Concat(AccessibleServers(obj)));

                default:
                    return Enumerable.Empty<Server>();
            }
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
            switch (node.ClassName) {
                case Helper.ResourceClass:
                    return (true, new Resource(Helper.NewResrcName()));
                case Helper.LayerClass:
                    return (true, new Layer());
                case Helper.ServerClass:
                    if (stackC.Count > 1) {
                        _log.LogWarning(
                            Helper.LogPref + "the server instance is not in the global context, it might not be considered for running.",
                            node.Token.Line,
                            node.Token.Column
                            );
                    }
                    return (true, new Server(Helper.NewServerName(), Helper.Logger<Server>(), Helper.Logger<Status>()));
                default:
                    _log.LogError(
                        Helper.LogPref + "an instance of the class {c} can't be created.",
                        node.Token.Line,
                        node.Token.Column,
                        node.ClassName);
                    return default;
            }
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