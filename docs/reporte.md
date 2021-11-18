<h1> Game of Servers </h1>

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

## OneLeaderFollowersSimulator
<!--@remind cuan2 t senyale algo, lo voy a hacer so'lo una vez, no voy a repetir lo mismo cada vez q vea de nuevo el objetivo del senyalamiento-->
Simulación de un l&iacute;der con un conjunto de servidores en paralelo.
### Variables
- #### Variables de tiempo
  - $ t $ - tiempo general.
  - $ t_{A_1} $ - siguiente tiempo de arribo al lider <!--@audit líder va con tilde. Arregla puripallá abajo-->.
  - $ t_{A_2} $ - siguiente tiempo de arribo a los seguidores.
  - $ t_{D_i} $ - siguiente tiempo de salida del i-esimo <!--@audit ésimo va con tilde. Arregla puripallá abajo--> seguidor.
- #### Variables contadoras
  - $ N_A $ - cantidad de arribos 
  - $ N_D $ - cantidad de partidas 
  - $ A_1 $ - Diccionario de tiempos de arribo al lider
  - $ A_{d_i} $ - Lista de diccionarios donde $ A_{d_i}[j]= t_j $, siendo  $ A_{d_i} $ el diccionario correspondiente al i-esimo seguidor y $ t_j $ el tiempo de partida asociado al 'cliente' j-esimo. <!--@audit esto de la lista no c entiend mucho, o sea, 1ro dices q es una lista y luego q es un diccionario-->
- #### Variables de estado
  - $ n_1 $ - número de clientes en el lider.
  - $ n $ - número de clientes en el sistema.
  - $ F_s $ - servidores libres.
  - $ q $ - cantidad de 'clientes' esperando en la cola de los seguidores.

### Eventos
- **Arribo al lider**  $( t_{A_1} == min( t_{A_1},t_{A_2}, t_{D_1},t_{D_2},... ) \wedge t_{A_1} < T ) $ :
  
  - $ t = t_{A_1} $
  - $ N_A = N_A + 1 $
  - $ n_1 = n_1 + 1 $
  - $ n = n + 1 $ <!--@audit t falta la línea 94 d OneLeaderFollowersSimulator q va despue's d esta-->
  - $ if~(n_1 == 1)~~~then~~~~(~t_{A_S}=genArrivalTimeOffset()<!--@audit este ) ta d +. También es mejor q lo hagas como en la conf, q cogen y dicen: generar t_{A_s} y... -->)~\wedge ~t_{A_2}=t + t_{A_S})$
  - $ A_1[N_A] = t$
- **Arribo a los seguidores**  $( t_{A_2} == min( t_{A_1},t_{A_2}, t_{D_1},t_{D_2},... ) \wedge t_{A_2} < T )$ :
  - $ t = t_{A_2} $
  - $ n_1=n_1-1 $
  - $ if~(n_1 \ne 0)~~~then~~~~(~t_{A_S}=genArrivalTimeOffset()~\wedge ~t_{A_2}=t + t_{A_S})$
  - $else~~~~t_{A_2} = \infin $
  - $ if~(|F_s| == 0)~~~then~~~~( q = q+1)$
  - $else:$
    - $ serv = F_s.Dequeue() $ 
    - $ client = N_A - n_1 $ <!--@audit a partir d aki' c enreda la cosa. Haz como en las dema's actualizaciones d tiempo: generar fulano y sumarlo a mengano, y en el caso del cliente, decir: inserto $client$ en $serv$. Recuerda q es pseudoco'digo, no hay q detallar tanto-->
    - $ t_{D_{Offset}} = genDeapTimeOffset() $
    - $ t_{D_{serv,client}} = t + t_{D_{Offset}} $  <!-- @note esta nota es d Mauricio: incluir info del cliente aca -->
- **Partida** $(min(t_{D_1},t_{D_2},...)==min( t_{A_1},t_{A_2}, t_{D_1},t_{D_2},... )) \wedge (min(t_{D_1},t_{D_2},...) \le T$:
  - $t_{Dmin}= GetMinDep()$ <!--@todo sustituye GetMinDep() x \min(t_{D_1},t_{D_2},...), pa no tener q explicar kie'n es esa funcio'n-->
  - $serv = GetMinServDep()$ <!--@todo sustituye por: obtener servidor de la partida-->
  - $client = GetMinClientDep()$ <!--@todo sustituye por: obtener cliente q part-->
  - $ t = t_{t_Dmin}$ <!--@audit hay una t d + ahi'-->
  - $ N_D = N_D +1$
  - $ n=n-1 $
  - $if~(q \ne 0)~~~then$ :
    - $q=q+1$ <!--@audit aki' lo q va es una resta-->
    - $client = N_A-q$
    - $ t_{D_{Offset}} = genDeapTimeOffset() $
    - $ t_{D_{serv,client}} = t + t_{D_{Offset}} $
  - $else ~~~~F_s.Add(serv)$
  - $A_{d_{serv}}[client]= t_{Dmin} $
- **Arribo fuera de tiempo para el lider** $ (t_{A_1}\ne \infin \wedge t_{A_1} ==  min( t_{A_1},t_{A_2}, t_{D_1},t_{D_2},... ) \wedge  t_{A_1} >T) $:
  - $ t_{A_1} = \infin $
- **Arribo fuera de tiempo para los seguidores**  $ (t_{A_2}\ne \infin \wedge t_{A_2} ==  min( t_{A_1},t_{A_2}, t_{D_1},t_{D_2},... ) \wedge  t_{A_1} >T) $ :
  - $ t_{A_2} = \infin $
- **Cierre** $(min(t_{D_1},t_{D_2},...)==min( t_{A_1},t_{A_2}, t_{D_1},t_{D_2},... ))~~\wedge~~((min(t_{D_1},t_{D_2},...) > T)~~\wedge$

  $((min(t_{D_1},t_{D_2},...) \ne \infin)~~\wedge~~n>0$ :
  
  El evento de cierre es análogo al evento de partida.

  