grammar FPTGrammar;

/*
 * Parser Rules
 */

/*stat : returnstat | assign | ifstat ;
returnstat : 'return' expr ';' ;
assign : 'x' '=' expr ;
ifstat : 'if' expr 'then' stat ;
expr : 'x' '+' '0' | 'x' '>' '0' | '0' ;*/

expr: multExpr ('+' multExpr)* ;
multExpr: primary (('*'|'.') primary)* ;
primary
	:	INT
	|	ID
	|	'[' expr(',' expr)* ']'
	;

list : '[' elements ']' ;
elements : element (',' element)* ;
element : NAME | list ;

/*
 * Lexer Rules
 */

fragment LOWERCASE : [a-z] ;
fragment UPPERCASE : [A-Z] ;

WORD : (LOWERCASE | UPPERCASE)+ ;
TEXT : '"' .*? '"' ;
WHITESPACE : (' '|'\t')+ -> skip ;
NEWLINE : ('\r'? '\n' | '\r')+ ;
