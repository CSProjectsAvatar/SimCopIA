<h1> Game of Servers </h1>

- [Propuesta](#propuesta)
- [1ra Entrega](#1ra-entrega)
  - [Ejecutando `gos`](#ejecutando-gos)
  - [Modelo de un Líder y muchos Seguidores](#modelo-de-un-líder-y-muchos-seguidores)
    - [Variables](#variables)
    - [Eventos](#eventos)
- [2da Entrega](#2da-entrega)
  - [Gram&aacute;tica](#gramática)
  - [Reglas Sem&aacute;nticas](#reglas-semánticas)

## Propuesta

Somos

* Claudia Puentes Hernández ([@ClauP99](https://github.com/ClauP99)) :bee:,
* Omar Alejandro Hernández Ramírez ([@OmarHernandez99](https://github.com/OmarHernandez99)) :tiger:,
* Andy Ledesma García ([@MakeMake23](https://github.com/MakeMake23)) :wolf: y
* Mauricio Mahmud Sánchez ([@maux96](https://github.com/maux96)) :fox_face:

y proponemos que el proyecto conjunto de Simulación, Compilación e IA sea sobre servidores y se llame ***Game of Servers***.

La idea va de simular un entorno con una cantidad determinada de servidores y un número potencialmente infinito de clientes. Los clientes emitirán pedidos a los servidores y estos responderán en consecuencia :grin: o no :pensive:, como sucede en la realidad.

El usuario de nuestro proyecto podrá programar cada uno de los servidores para que responda a los pedidos según crea conveniente. Esto se realizará en un lenguaje creado por nosotros para este dominio específico :sunglasses:.

Un servidor también puede emitir pedidos a otro servidor :scream:, convirtiéndose el primero en un cliente del segundo. En este sentido, se pudieran aplicar algoritmos de IA :astonished: para enrutar el pedido de forma óptima entre servidores.

En un sistema como este se pueden simular:
* ataques DoS y DDoS
* pérdidas de usuarios y capital en servicios online por demora en las respuestas
* distintas estrategias de ruteo y de distribución de carga
* el accionar de cada uno de servers, como agentes autónomos
* la viabilidad del sistema en conjunto en cuanto a  la tolerancia a fallas, alta disponibilidad.

Incluyendo IA allá donde puede ser más útil :grin:.

## 1ra Entrega
En la primera entrega del proyecto simulamos el procesamiento de pedidos en un sistema compuesto por un servidor (repartidor de carga) que selecciona cu&aacute;l de los servidores restantes (*doers*) se encargar&aacute; de procesar el pedido entrante. En la secci&oacute;n [Modelo de un Líder y muchos Seguidores](#modelo-de-un-líder-y-muchos-seguidores) se explica en detalle c&oacute;mo modelamos este sistema.

Esta simulaci&oacute;n es ejecutada m&uacute;ltiples veces por un algoritmo gen&eacute;tico, con el objetivo de determinar el número de *doers* necesarios para minimizar el tiempo de respuesta a los pedidos.

El algoritmo gen&eacute;tico, a su vez, es ejecutado por una aplicaci&oacute;n de consola llamada `gos` que recibe sus par&aacute;metros del archivo `appsettings.json`. En la secci&oacute;n [Ejecutando gos](#ejecutando-gos) se explica c&oacute;mo se ejecuta el programa y el significado de cada par&aacute;metro.

### Ejecutando `gos`
Para ejecutar nuestro programa, descargue el *release* para su sistema operativo y abra el archivo `gos` (Linux) o `gos.exe` (Windows) desde una terminal.

Los par&aacute;metros deben ser configurados en el archivo `appsettings.json`. Estos son
- `Followers`: cantidad de *doers*.
- `Lambda`: parámetro lambda de la distribución exponencial para determinar tiempos de ocurrencia de los eventos.
- `CloseTime`: tiempo de cierre del sistema ($T$). Cuando se arribe a este tiempo, no se recibirán más pedidos.
- `MonthlyMaintenanceCost`: costo mensual máximo de mantenimiento del sistema.
- `RunTimeMilliseconds`: tiempo en milisegundos de corrida de la metaheurística.
- `Poblation`: número de individuos del algoritmo genético.


### Modelo de un Líder y muchos Seguidores
El sistema de la simulaci&oacute;n fue modelado mediante dos capas conectadas en serie: la del repartidor de carga (l&iacute;der) y la de los *doers* (seguidores). Estos &uacute;ltimos procesan los pedidos en paralelo.

A continuaci&oacute;n se definen las variables y los eventos de la simulaci&oacute;n.
#### Variables
- Variables de tiempo
  - $ t $ - tiempo general.
  - $ t_{A_1} $ - siguiente tiempo de arribo al líder.
  - $ t_{A_2} $ - siguiente tiempo de arribo a los seguidores.
  - $ t_{D_i} $ - siguiente tiempo de salida del i-ésimo seguidor.
- Variables contadoras
  - $ N_A $ - cantidad de arribos 
  - $ N_D $ - cantidad de partidas 
  - $ A_1 $ - Diccionario de tiempos de arribo al líder
  - $ A_{d_x} $ - Lista de diccionarios donde $ A_{d_i}[j]= t_j $, siendo  $ A_{d_i} $ el diccionario correspondiente al i-ésimo seguidor y $ t_j $ el tiempo de partida asociado al 'cliente' j-ésimo. 
- Variables de estado
  - $ n_1 $ - número de clientes en el líder.
  - $ n $ - número de clientes en el sistema.
  - $ F_s $ - servidores libres.
  - $ q $ - cantidad de 'clientes' esperando en la cola de los seguidores.

#### Eventos
- **Arribo al líder**  $( t_{A_1} == min( t_{A_1},t_{A_2}, t_{D_1},t_{D_2},... ) \wedge t_{A_1} < T ) $ :
  
  - $ t = t_{A_1} $
  - $ N_A = N_A + 1 $
  - $ n_1 = n_1 + 1 $
  - $ n = n + 1 $ 
  - $ generar~~t_{A_{L}} \wedge~~t_{A_1} = t + t_{A_{L}} $ 
  - $ if~(n_1 == 1)~~~then~~~~generar~~t_{A_S}~\wedge ~t_{A_2}=t + t_{A_S}$   
  - $ A_1[N_A] = t$
- **Arribo a los seguidores**  $( t_{A_2} == min( t_{A_1},t_{A_2}, t_{D_1},t_{D_2},... ) \wedge t_{A_2} < T )$ :
  - $ t = t_{A_2} $
  - $ n_1=n_1-1 $
  - $ if~(n_1 \ne 0)~~~then~~~~(generar~t_{A_S}~\wedge ~t_{A_2}=t + t_{A_S})$
  - $else~~~~t_{A_2} = \infin $
  - $ if~(|F_s| == 0)~~~then~~~~( q = q+1)$
  - $else:$
    - $ serv = F_s.Dequeue() $ 
    - $ client = N_A - n_1 $
    - $ generar~t_{D_S}~~\wedge~~t_{D_{serv}} =t + t_{D_S}$
    - se inserta $client$ en $serv$ 

- **Partida** $(min(t_{D_1},t_{D_2},...)==min( t_{A_1},t_{A_2}, t_{D_1},t_{D_2},... )) \wedge (min(t_{D_1},t_{D_2},...) \le T$:
  - $t_{Dmin}=min(t_{D_1},t_{D_2},...)$
  - $serv = ObtenerServidorPartida()$
  - $client = OptenerClienteQueParte()$ 
  - $ t = t_{Dmin}$
  - $ N_D = N_D +1$
  - $ n=n-1 $
  - $if~(q \ne 0)~~~then$ :
    - $q=q-1$ 
    - $client = N_A-q$
    - $generar~t_{D_{S}} ~~\wedge~~ t_{D_{serv,client}} = t + t_{D_{S}}  $
  - $else ~~~~F_s.Add(serv)$
  - $A_{d_{serv}}[client]= t_{Dmin} $
- **Arribo fuera de tiempo para el líder** $ (t_{A_1}\ne \infin \wedge t_{A_1} ==  min( t_{A_1},t_{A_2}, t_{D_1},t_{D_2},... ) \wedge  t_{A_1} >T) $:
  - $ t_{A_1} = \infin $
- **Arribo fuera de tiempo para los seguidores**  $ (t_{A_2}\ne \infin \wedge t_{A_2} ==  min( t_{A_1},t_{A_2}, t_{D_1},t_{D_2},... ) \wedge  t_{A_1} >T) $ :
  - $ t_{A_2} = \infin $
- **Cierre** $(min(t_{D_1},t_{D_2},...)==min( t_{A_1},t_{A_2}, t_{D_1},t_{D_2},... ))~~\wedge~~((min(t_{D_1},t_{D_2},...) > T)~~\wedge$

  $((min(t_{D_1},t_{D_2},...) \ne \infin)~~\wedge~~n>0$ :
  
  El evento de cierre es análogo al evento de partida.

## 2da Entrega
### Gram&aacute;tica
```
<program> := <stat-list>

<stat-list> := <stat> ";"
             | <stat> ";" <stat-list>
        	   | <block-stat>
             | <block-stat> <stat-list>
             
<block-stat> := <if>
              | <def-func>

<stat> := <let-var>
        | <print-stat>
        | <return>

<let-var> := "let" ID "=" <expr>

<def-func> := "fun" ID "(" <arg-list> ")" "{" <stat-list> "}"

<print-stat> := "print" <expr>

<arg-list> := ID
            | ID "," <arg-list>

<expr> := <math> "<" <math>
		| <math> ">" <math>
		| <math>

<math> := <math> "+" <term>
        | <math> "-" <term>
        | <term>

<term> := <term> "*" <factor>
        | <term> "/" <factor>
        | <factor>

<factor> := <atom>
          | "(" <math> ")"

<atom> := NUMBER
        | ID
        | <func-call>

<func-call> := ID "(" <expr-list> ")"

<expr-list> := <expr>
             | <expr> "," <expr-list>
             
<if> := "if" <expr> "{" <stat-list> "}"

<return> := "return" <expr>
```
**El `;` lo pone el *lexer***, no es necesario que el usuario lo haga. Este puede emplear `\` para definir *statements* de m&aacute;s de una l&iacute;nea.

### Reglas Sem&aacute;nticas
Una variable solo puede ser definida una vez en todo el
programa.
- Los nombres de variables y funciones no comparten el mismo
ámbito (pueden existir una variable y una función llamadas
igual).
- No se pueden redefinir las funciones predefinidas.
- Una función puede tener distintas definiciones siempre que
tengan distinta cantidad de argumentos.
- Toda variable y función tiene que haber sido definida antes de
ser usada en una expresión (salvo las funciones pre-definidas).
- Todos los argumentos definidos en una misma función tienen
que ser diferentes entre sí, aunque pueden ser iguales a
variables definidas globalmente o a argumentos definidos en
otras funciones.
- En el cuerpo de una función, los nombres de los argumentos
ocultan los nombres de variables iguales.