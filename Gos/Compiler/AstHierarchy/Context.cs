
using System;
using System.Collections.Generic;
using Agents;
namespace DataClassHierarchy
{
    public class Context
    {
        public static Agents.Environment Simulation {get; internal set;}
        Context parent;
        
        Dictionary<string, object> _variables;
        Dictionary<(string, int), DefFun> _functions; // (nombre, aridad) 
        public Context(){
            _variables = new Dictionary<string, object>();
            _functions = new Dictionary<(string, int), DefFun>();

        }
        public bool DefVariable(string name, object value = null)
        {
            if (_variables.ContainsKey(name))
            {
                return false;
            }
            _variables[name] = value;
            return true;
        }

        public void SetVar(string name, object value) {
            for (var ctx = this; ctx != null; ctx = ctx.parent) {
                if (ctx._variables.ContainsKey(name)) {
                    ctx._variables[name] = value;
                    break;
                }
            }
        }

        public bool DefFunc(string name, int funArity, DefFun fun = null){
            if(_functions.ContainsKey((name, funArity))){
                return false;
            }
            SetFunc(name, funArity, fun);
            return true;
        }

        public void SetFunc(string name, int funArity, DefFun fun)
        {
            _functions[(name, funArity)] = fun;
        }

        public bool CheckVar(string varName){
            return _variables.ContainsKey(varName) || (parent != null && parent.CheckVar(varName));
        }
        public bool CheckFunc(string funcName, int arity){
            return _functions.ContainsKey((funcName, arity)) 
                || (parent != null && parent.CheckFunc(funcName, arity));
        } 
        
        public object GetVar(string varName){
            if(_variables.ContainsKey(varName)){
                return _variables[varName];
            }
            if(parent != null){
                return parent.GetVar(varName);
            }
            return null;
        }
        public DefFun GetFunc(string funcName, int arity){
            if(_functions.ContainsKey((funcName, arity))){
                return _functions[(funcName, arity)];
            }
            if(parent != null){
                return parent.GetFunc(funcName, arity);
            }
            return null;
        }
            


        public Context CreateChildContext(){
            var childContent = new Context();
            childContent.parent = this;
            
            return childContent;
        }
    }
}