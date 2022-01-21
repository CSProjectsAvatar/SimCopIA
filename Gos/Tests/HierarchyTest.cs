using Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.IO;

namespace Tests {
    [TestClass]
    public class HierarchyTest : BaseTest {
        private ILogger<HierarchyTest> _log;
        private Context global;
        private EvalVisitor evalVisitor;

        [TestInitialize]
        public void Init() {
            global = new Context();
            _log = LoggerFact.CreateLogger<HierarchyTest>();
            var _evallog = LoggerFact.CreateLogger<EvalVisitor>();
            evalVisitor = new EvalVisitor(global, _evallog, new StringWriter());
        }

        [TestMethod]
        public void EvaluatingNumber() {
            var n = new Number(){ Value = "5"};
            var (success, result) = evalVisitor.Visit(n);
            Assert.IsTrue(success);
            Assert.IsInstanceOfType(result, typeof(double));
            Assert.IsTrue(Math.Abs(5 - (double)result) <= 0.001);
        }

        [TestMethod]
        public void EvaluatingLetVar() {
            var letA = new LetVar(){ 
                Identifier = "a",
                Expr = new Number(){ Value = "10"}
             };

            var (success, result) = evalVisitor.Visit(letA);
            
            Assert.IsTrue(success);
            Assert.IsInstanceOfType(result, typeof(double));

            var dA = global.GetVar(letA.Identifier);
            Assert.AreEqual((double)10, dA);
        }
    
        [TestMethod]
        public void EvaluatingVariable() {
            var letA = new LetVar(){ 
                Identifier = "a",
                Expr = new Number(){ Value = "10"}
             };
            var letB = new LetVar(){ 
                Identifier = "b",
                Expr = new Variable(){ Identifier = "a"}
             };
            
            var prog = new ProgramNode(){
                Statements = new List<IStatement>(){
                    letA,
                    letB
                }
            };
            var (success, result) = evalVisitor.Visit(prog);
            
            Assert.IsTrue(success);

            var dA = global.GetVar(letA.Identifier);
            Assert.AreEqual((double)10, dA);
            
            var dB = global.GetVar(letB.Identifier);
            Assert.AreEqual((double)10, dB);
        }
    
        [TestMethod]
        public void EvaluatingDefFunAndFunCall() {
        #region Program
            var defDouble = new DefFun(){ 
                Identifier = "Double",
                Arguments = new List<string>(){ "x"},
                Body = new List<IStatement>(){
                    new Return(){
                        Expr =  new MultOp(){
                            Left = new Number(){ Value = "2"},
                            Right = new Variable(){ Identifier = "x"}
                        }
                    }
                } 
            };
            var letA = new LetVar(){ 
                Identifier = "a",
                Expr = new Number(){ Value = "10"}
             };
            var funCallDoubleA = new FunCall(){
                Identifier = "Double",
                Args = new List<Expression>(){ new Variable(){ Identifier = "a"}}
            };
            var letB = new LetVar(){  // b = Double a
                Identifier = "b",
                Expr = funCallDoubleA
             };
            var prog = new ProgramNode(){
                Statements = new List<IStatement>(){
                    defDouble, // f x -> 2x
                    letA,      // a = 10
                    letB       // b = Double a
                }
            };
        #endregion

            var valid = prog.Validate(global);
            Assert.IsTrue(valid);

            var (success, result) = evalVisitor.Visit(prog);
            Assert.IsTrue(success);

            var dA = global.GetVar(letA.Identifier);
            Assert.AreEqual((double)10, dA);
            
            var dB = global.GetVar(letB.Identifier);
            Assert.AreEqual((double)20, dB);
        }

        [TestMethod]
        public void EvaluatingEqEqOperand() {
        #region Program
        
            var letA = new LetVar(){ 
                Identifier = "a",
                Expr = new Number(){ Value = "10"}
             };
            var letB = new LetVar(){ 
                Identifier = "b",
                Expr = new Number(){ Value = "10"}
             };
            var letC = new LetVar(){ 
                Identifier = "c",
                Expr = new Number(){ Value = "11"}
             };
            var letD = new LetVar(){ 
                Identifier = "d", // d = a == b (true)
                Expr = new EqEqOp(){
                    Left = new Variable(){ Identifier = "a"},
                    Right = new Variable(){ Identifier = "b"}
                }
             };
            var letE = new LetVar(){ 
                Identifier = "e", // e = a == c (false)
                Expr = new EqEqOp(){
                    Left = new Variable(){ Identifier = "a"},
                    Right = new Variable(){ Identifier = "c"}
                }
             };

            var prog = new ProgramNode() {
                Statements = new List<IStatement>() {
                    letA,       // a = 10
                    letB,       // b = 10
                    letC,       // c = 11
                    letD,       // d = a == b (true)
                    letE        // e = a == c (false)
                }
            };
            
        #endregion

            var valid = prog.Validate(global);
            Assert.IsTrue(valid);

            var (success, result) = evalVisitor.Visit(prog);
            Assert.IsTrue(success);

            var dD = global.GetVar(letD.Identifier);
            Assert.IsTrue((bool)dD);

            var dE = global.GetVar(letE.Identifier);
            Assert.IsFalse((bool)dE);
        }
    
        [TestMethod]
        public void EvalFunMultiLine() {
        #region Program
            var defDoubleMinusOne = new DefFun(){ 
                Identifier = "DoubleMinusOne",
                Arguments = new List<string>(){ "x"},
                Body = new List<IStatement>(){
                    new LetVar(){ // y = 1
                        Identifier = "y",
                        Expr = new Number(){ Value = "1"}
                    },
                    new Return(){ // return 2x - y
                        Expr = new SubOp(){
                            Left = new MultOp(){
                                Left = new Number(){ Value = "2"},
                                Right = new Variable(){ Identifier = "x"}
                            },
                            Right = new Variable(){ Identifier = "y"}
                        } 
                    }
                } 
            };
            var letA = new LetVar(){ 
                Identifier = "a",
                Expr = new Number(){ Value = "10"}
             };
            var funCalldefDoubleMinusOne = new FunCall(){
                Identifier = "DoubleMinusOne",
                Args = new List<Expression>(){ new Variable(){ Identifier = "a"}}
            };
            var letB = new LetVar(){  // b = DoubleMinusOne a
                Identifier = "b",
                Expr = funCalldefDoubleMinusOne
             };
            var prog = new ProgramNode(){
                Statements = new List<IStatement>(){
                    defDoubleMinusOne, // f x -> { y = 1; return 2x - y }
                    letA,      // a = 10
                    letB       // b = DoubleMinusOne a
                }
            };
        #endregion

            var valid = prog.Validate(global);
            Assert.IsTrue(valid);

            var (success, result) = evalVisitor.Visit(prog);
            Assert.IsTrue(success);

            var dA = global.GetVar(letA.Identifier);
            Assert.AreEqual((double)10, dA);
            
            var dB = global.GetVar(letB.Identifier);
            Assert.AreEqual((double)19, dB);
        }

        [TestMethod]
        public void EvalRecFun() {
        #region Program
            var defFactorial = new DefFun(){ 
                Identifier = "Factorial",
                Arguments = new List<string>(){ "x"},
                Body = new List<IStatement>(){
                    new IfStmt(){
                        Condition = new LessThanOp(){ // if x < 1
                            Left = new Variable(){ Identifier = "x"},
                            Right = new Number(){ Value = "1"}
                        },
                        Then = new List<IStatement>(){
                            new Return(){
                                Expr = new Number(){ Value = "1"}
                            }
                        }
                    },
                    new Return(){ // else return x * Factorial(x - 1)
                        Expr = new MultOp(){
                            Left = new Variable(){ Identifier = "x"},
                            Right = new FunCall(){
                                Identifier = "Factorial",
                                Args = new List<Expression>(){
                                    new SubOp(){
                                        Left = new Variable(){ Identifier = "x"},
                                        Right = new Number(){ Value = "1"}
                                    }
                                }
                            }
                        }
                    }
                } 
            };
            var letA = new LetVar(){ 
                Identifier = "a",
                Expr = new Number(){ Value = "5"}
             };
            var funCallFact = new FunCall(){
                Identifier = "Factorial",
                Args = new List<Expression>(){ new Variable(){ Identifier = "a"}}
            };
            var letB = new LetVar(){  // b = Factorial a
                Identifier = "b",
                Expr = funCallFact
            };
            var prog = new ProgramNode(){
                Statements = new List<IStatement>(){
                    defFactorial, // f x -> x * f (x-1)
                    letA,      // a = 5
                    letB       // b = Fact a
                }
            };
        #endregion

            var valid = prog.Validate(global);
            Assert.IsTrue(valid);

            var (success, result) = evalVisitor.Visit(prog);
            Assert.IsTrue(success);

            var dA = global.GetVar(letA.Identifier);
            Assert.AreEqual((double)5, dA);
            
            var dB = global.GetVar(letB.Identifier);
            Assert.AreEqual((double)120, dB);
        }
    
        // Test for the recursive function Fibbonacci
        [TestMethod]
        public void EvalFibbonacci() {
            #region Program
            var defFib = new DefFun(){ 
                Identifier = "Fib",
                Arguments = new List<string>(){ "x"},
                Body = new List<IStatement>(){
                    new IfStmt(){
                        Condition = new LessThanOp(){ // if x < 2
                            Left = new Variable(){ Identifier = "x"},
                            Right = new Number(){ Value = "2"}
                        },
                        Then = new List<IStatement>(){
                            new Return(){
                                Expr = new Number(){ Value = "1"}
                            }
                        }
                    },
                    new Return(){ // else return Fib(x-1) + Fib(x-2)
                        Expr = new AddOp(){
                            Left = new FunCall(){
                                Identifier = "Fib",
                                Args = new List<Expression>(){
                                    new SubOp(){
                                        Left = new Variable(){ Identifier = "x"},
                                        Right = new Number(){ Value = "1"}
                                    }
                                }
                            },
                            Right = new FunCall(){
                                Identifier = "Fib",
                                Args = new List<Expression>(){
                                    new SubOp(){
                                        Left = new Variable(){ Identifier = "x"},
                                        Right = new Number(){ Value = "2"}
                                    }
                                }
                            }
                        }
                    }
                } 
            };
            var letA = new LetVar(){ // a = 6
                Identifier = "a",
                Expr = new Number(){ Value = "6"}
             };
            var funCallFib = new FunCall(){
                Identifier = "Fib",
                Args = new List<Expression>(){ new Variable(){ Identifier = "a"}}
            };
            var letB = new LetVar(){  // b = Fib a
                Identifier = "b",
                Expr = funCallFib
            };
            var prog = new ProgramNode(){
                Statements = new List<IStatement>(){
                    defFib, // f x -> if x < 2 then 1 else f(x-1) + f(x-2)
                    letA,      // a = 6
                    letB       // b = Fib a
                }
            };
        #endregion
                
            var valid = prog.Validate(global);
            Assert.IsTrue(valid);

            var (success, result) = evalVisitor.Visit(prog);
            Assert.IsTrue(success);

            var dA = global.GetVar(letA.Identifier);
            Assert.AreEqual((double)6, dA);

            var dB = global.GetVar(letB.Identifier);
            Assert.AreEqual((double)13, dB);
        }

    
    }
}
