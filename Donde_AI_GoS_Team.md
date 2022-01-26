Este es el documento para darle respuesta a la pregunta: "Donde usar AI en mi proyecto?"

Dado que el proyecto se pudiera resumir en un conjunto de agentes con funciones de comportamiento asignadas que tratan de sostener lo mejor posible un servicio web, algunos lugares donde se pudiera usar AI serian:

- Decidiendo las funciones de comportamiento a asignar a cada agente:
    Puede ser conveniente que las reglas de los propios agentes no lo decida un humano, sino una IA, de un espectro de comportamientos distintos ya predefinidos

- Elaborando la arquitectura de los agentes:
    En general, de acuerdo a sus comportamientos los agentes pueden pertenecer a 3 grupos, no exclusivos:
    - distribuidores de carga (DC)
    - workers (W)
    - hosteadores de datos (HD)

    Luego, se pueden hacer distintas arquitecturas como que 2 HD esten unidos por un DC al resto de la red, y reparta los pedidos a base de datos entre ambos servers

        W1                      --> HD_1
             ...      ->  DC -|
        Wn                      --> HD_2
        
    Todo esto respetando limites como puede ser un presupuesto inicial, y con objetivo de optimizar la velocidad de respuesta, o la confiabilidad de la red completa ante fallas.

    
