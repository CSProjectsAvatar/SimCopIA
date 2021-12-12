
using System.Collections.Generic;

namespace DataClassHierarchy
{
    public class Context
    {
        Context parent;
        Dictionary<string, Variable> _variables;
        Dictionary<(string, int), DefFun> _functions; // (nombre, aridad) -> DefFun
        public Context(){
            _variables = new Dictionary<string, Variable>();
            _functions = new Dictionary<(string, int), DefFun>();

        }
        public void DefVariable(string name, Variable value){ // @audit Guardamos los values por fin?
            _variables.Add(name, value);
        }
        public void DefFunc(string name, List<string> args, DefFun value){
            _functions.Add(name, value); // @audit Hay que decidirse sobre si guardar (name, aridad) o (name, args)
        }
        
        public bool CheckVar(string varName){
            return _variables.ContainsKey(varName) || (parent != null && parent.CheckVar(varName));
        }
        public bool CheckFunc(string funcName, int arity){
            return _functions.ContainsKey((funcName, arity)) 
                || (parent != null && parent.CheckFunc(funcName, arity));
        } 
        
        public Context CreateChildContext(){
            var childContent = new Context();
            childContent.parent = this;
            
            return childContent;
        }
    }
}