
namespace DataClassHierarchy
{
    // Notas:
    /*
    Suponiendo que vienen una lista de tokens pertencientes al lenguage de regex
    Se le alimenta al Creador de Parsers(LR1), la gramatica especifica de nuestros regex
    Luego, para que el LR1 nos de el Ast, necesita las clases que llevan del ASC al AST
    Una vez aki tenemos el Ast de los regex
    Hacemos la clase Automata, que sabe como hacer las operaciones basicas como union, concatenacion, etc
    Luego, con un Visitor, pasamos por el Ast y creamos el automata que reconoce una cadena dado el regex



Gramatica de regex:
<RE> ::=  <union> | <simple-RE>

<union>  ::= <RE> "|" <simple-RE>

<simple-RE>  ::=  <concatenation> | <basic-RE>

<concatenation>  ::= <simple-RE> <basic-RE>

<basic-RE> ::= <star> | <plus> | <elementary-RE>

<star> ::= <elementary-RE> "*"

<plus> ::= <elementary-RE> "+"

<elementary-RE>    ::= <group> | <any> | <eos> | <char> | <set>

<group> ::= "(" <RE> ")"

<any>  ::=     "."

<eos>  ::=     "$"

<char> ::=     any non metacharacter | "\" metacharacter

<set>  ::=     <positive-set> | <negative-set>

<positive-set> ::=     "[" <set-items> "]"

<negative-set> ::=     "[^" <set-items> "]"

<set-items>    ::=     <set-item> | <set-item> <set-items>

<set-items>    ::=     <range> | <char>

<range>    ::=     <char> "-" <char>

expression = term

             term | expression

term = factor

       factor term

factor = atom

         atom metacharacter

atom = character

       .

       ( expression )

       [ characterclass ]

       [ ^ characterclass ]

       { min }

       { min ,  }

       { min , max }

characterclass = characterrange

                 characterrange characterclass

characterrange = begincharacter

                 begincharacter - endcharacter

begincharacter = character

endcharacter = character

character =

            anycharacterexceptmetacharacters

            \ anycharacterexceptspecialcharacters

metacharacter = ?

                * {=0 or more, greedy}

                *? {=0 or more, non-greedy}

                + {=1 or more, greedy}

                +? {=1 or more, non-greedy}

                ^ {=begin of line character}

                $ {=end of line character}

                $` {=the characters to the left of the match}

                $' {=the characters to the right of the match}

                $& {=the characters that are matched}

                \t {=tab character}

                \n {=newline character}

                \r {=carriage return character}

                \f {=form feed character}

                \cX {=control character CTRL-X}

                \N {=the characters in Nth tag (if on match side)}

                $N{=the characters in Nth tag (if not on match side)}

                \NNN {=octal code for character NNN}

                \b {=match a 'word' boundary}

                \B {=match not a 'word' boundary}

                \d {=a digit, [0-9]}

                \D {=not a digit, [^0-9]}

                \s {=whitespace, [ \t\n\r\f]}

                \S {=not a whitespace, [^ \t\n\r\f]}

                \w {='word' character, [a-zA-Z0-9_]}

                \W {=not a 'word' character, [^a-zA-Z0-9_]}

                \Q {=put a quote (de-meta) on characters, until \E}

                \U {=change characters to uppercase, until \E}

                \L {=change characters to uppercase, until \E}

min = integer

max = integer

integer = digit

          digit integer

anycharacter = ! " # $ % & ' ( ) * + , - . / :
               ; < = > ? @ [ \ ] ^ _ ` { | } ~
               0 1 2 3 4 5 6 7 8 9
               A B C D E F G H I J K L M N O P Q R S T U V W X Y Z
               a b c d e f g h i j k l m n o p q r s t u v w x y z

---
    */
}