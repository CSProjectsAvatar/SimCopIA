using Compiler;
using Compiler.Lexer;

namespace Compiler.Lexer.Tests {
    public class LexerBaseTests : CompilerTests {
        protected static Grammar RegexGram => new ReGrammar();
    }
}