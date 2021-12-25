
using Compiler;

namespace DataClassHierarchy  // @audit NO TIENE NA Q VER EL NAMESPACE ESTE CON LA SOLUCIO'N, MI'NIMO DEBE SER Compiler.Parsing.DataClassHierarchy
{
    public abstract class AstNode : IValidable
    {
        public abstract bool Validate(Context context);

        /// <summary>
        /// Token relacionado con el chequeo sem�ntico.
        /// </summary>
        public Token Token { get; init; }  // esta propiedad es necesaria para tener una noci�n de en qu� parte del c�digo se est� haciendo el chequeo sem�ntico. Pa la retroalimentacio'n d sema'nticos esta' espectacular
        // @remind RECUERDA SETEARLA EN EL Unterminal.SetAst() D LOS Q INSTANCIEN UN AstNode Q DE FEEDBACK CUAN2 VALIDA
    }

    public interface IValidable
    {
        bool Validate(Context context);
    }
}