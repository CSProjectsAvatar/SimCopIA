# Change Log
Todos los cambios notables de este proyecto se documentarán en este archivo.
El formato se basa en [*Keep a Changelog*](https://keepachangelog.com/en/1.0.0/), y este proyecto se adhiere a [*Semantic Versioning*](https://semver.org/spec/v2.0.0.html).
<!-- Check [Keep a Changelog](http://keepachangelog.com/) for recommendations on how to structure this file. -->

## 0.3.0
### Added
- Palabras claves `else_if` y `else`.
- Palabra clave `new`.

### Changed
- Color de los tipos `distw` y `simplew`: ahora es verde.

## 0.2.0
### Added
- Palabras claves `simplew` y `distw`.
- Operador `->`.
- Caracter&iacute;sticas principales del lenguaje en `README.md`:
  - Declaraci&oacute;n de funciones.
  - *Statements* en varias l&iacute;neas.
  - Crear servidores de tipo *worker* simple y distribuidores de carga.
  - Conexiones.
- GIFs para explicar lo dicho en el punto anterior.
- Enlace hacia el reporte en `README.md`.

## 0.1.1
### Fixed
- Imagen *demo* del README. Ten&iacute;a un formato de comentario que no se admite.
## 0.1.0
### Added
- Lo que se hizo en la versi&oacute;n [0.0.1](#v001) en el CHANGELOG.
### Changed
- Los comentarios de una l&iacute;nea se colocan con `#`.
- Soportamos el Visual Studio Code de su versi&oacute;n 1.62.0 en adelante.

### Removed
- No soportamos comentarios de bloque.

## 0.0.1
### Added
- *Syntax highlight* para el lenguaje GoS:
  -  comentarios
  -  *keywords* de control: `if`, `return`
  -  otros *keywords*: `fun`, `let`, `print`
  -  operadores aritm&eacute;ticos: `>`, `<`, `=`, `==`, `+`, `-`, `*`, `/`
  -  n&uacute;meros
  -  identificadores
  -  funciones
-  README
-  CHANGELOG
-  Autocompletamiento de caracteres grupales, como son `{`, `(`, etc.