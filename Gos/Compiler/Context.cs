
using System.Collections.Generic;

namespace DataClassHierarchy
{
    public class Context
    {
        Context parent;
        
        HashSet<string> _variables;
        HashSet<(string, int)> _functions; // (nombre, aridad) 
        public Context(){
            _variables = new HashSet<string>();
            _functions = new HashSet<(string, int)>();

        }
        public bool DefVariable(string name){
            if(_variables.Contains(name)){
                return false;
            }
            _variables.Add(name);
            return true;
        }
        public bool DefFunc(string name, int funArity){
            if(_functions.Contains((name, funArity))){
                return false;
            }
            _functions.Add((name, funArity));
            return true;
        }
        
        public bool CheckVar(string varName){
            return _variables.Contains(varName) || (parent != null && parent.CheckVar(varName));
        }
        public bool CheckFunc(string funcName, int arity){
            return _functions.Contains((funcName, arity)) 
                || (parent != null && parent.CheckFunc(funcName, arity));
        } 
        
        public Context CreateChildContext(){
            var childContent = new Context();
            childContent.parent = this;
            
            return childContent;
        }
    }
}