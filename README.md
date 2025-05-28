# Projecte de Sostenibilitat i Digitalització - Grup 3

El projecte està dividit en dues seccions, la part de la [api](https://github.com/AlejandroMarEst/ProjecteSOS_Grup03/tree/main/ProjecteSOS_Grup03API), i de la [web](https://github.com/AlejandroMarEst/ProjecteSOS_Grup03/tree/main/ProjecteSOS_Grup03WebPage).

En cada projecte de Visual Studio hi ha tota la configuració corresponent. La api està feta a partir de un projecte de ASP.NET Core Web Api i la web amb ASP.NET Core Web App (Razor Pages), tot en c#.

## Usuari per defecte

En entrar a la api hi ha un usuari que es crea per defecte, un usuari administrador.

```
User: admin@dynamx.com
Password: Admin1234!
```

## Api

### Models

Per a fer funcionar la api, necessita classes Model, aquests són els Models que té la api:

- **User**: Una classe que hereta d'IdentityUser per a utilitzar totes les propietats que conté aquesta.
- **Client**: Classe per a identificar els clients, hereta de User.
- **Employee**: Classe per a identificar els empleats, també administradors, hereta de User.
- **Product**: Classe per als productes amb totes les propietats necessaries per a poder gestionar-los correctament.
- **Order**: Classe per a les comandes realitzades contenint el client que l'ha fet, i si l'ordre s'ha fet en la botiga física el empleat que ha gestionat la copmanda, a més del preu d'aquesta.
- **ProductOrder**: Classe que es crea de la relació de molts-molts entre Order i Product, contenint els seus Ids.

### DbContext

El DbContext hereta de IdentityDbContext al utilitzar els usuaris amb Identity, aquest manega la base de dades, així com les taules i demés. En inicialitzar la base de dades aquesta es crea amb 5 productes per defecte.

### DTOs

La api conté una gran llista de DTOs basats en les classes Model, alguns són exactament iguals que la classe Model, però sense la propietat del Id, com **OrderDTO** o **ProductDTO**, i d'altres són completament diferents a la classe Model o treuen moltes propietats o creen de noves, com **LoginDTO** o **ProductOrderDetailsDTO**.

Aquests DTOs serveixen per a retornar la infromació justa i necessaria a qui la demani, sense proporcionar-li informació extra que no es necessaria, o per a obtenir les dades justes per a després complementar-les amb un DTO amb més dades que es basa en aquestes o d'altres obtingudes d'altres mètodes.

### Controladors

La api conté 4 controladors, cada un realitzen funcions diferents, com AuthController que s'encarrega de manegar tot el relacionat amb els usuaris, com el seu login o registre i l'obtenció d'un perfil d'usuari.

#### AuthController

Aquest controlador s'encarrega de tot el que està relacionat amb els usuaris, aquest permet registrar usuaris, tant clients, com administradors o emepleats, i permetre als usuaris registrats iniciar sessió amb el seu propi usuari. En iniciar sessió la api proporciona un token per a mantenir la sessió iniciada, la sessió perdurarà durant 1 hora, un cop passat el temps la sessió es tancarà i s'haura de tornar a iniciar sessió.

AuthController també s'encarrega dels perfils dels usuaris, aquest proporciona el perfil del teu propi usuari, o el perfil d'un usuari concret en el cas que siguis un administrador. Des del perfil pots canviar el nom, el telèfon i la contrasenya del teu propi usuari a més de poder eliminar el teu usuari.

#### OrdersController

El controlador de Orders s'encarrega del que està relacionat amb les comandes dels clients, aquest pot crear comandes i assignar-li a clients, en el cas de que siguis administrador, o crear una comanda en el teu usuari client. Al crear una comanda ja pots començar a demanar productes per a després confirmar la comanda, un altre funció d'aquest controlador. Quan un usuari té una comanda activa, la Id de la comanda es guarda en la base de dades en la entitat del usuari, un cop la comanda es confirma aquest Id desapareix, torna a null, tot això en el cas que sigui un usuari client que realitzi la comanda. En el cas que l'ordre la realitzi un usuari empleat aquesta se li assignarà a l'usuari client que ell desitgi, simulat una ordre en la bottiga física, el funcionament és quasi el mateix que el de la comanda online.

Les comandes també es poden modificar, eliminar i veure totes les comandes realitzades, però aquestes funcions només estàn disponibles per a administradors. Els usuaris anònims no poden fer res amb les comandes, ni crear ni modificar ni res.

#### ProductsController

El controladort de Products s'encarrega dels productes que hi ha a la base de dades, un usuari anònim només podrà veure els productes, mentre que un client afegir-los a una comanda, però afegir-los a una comanda no és part d'aquest controlador, un administrador si que podrà eliminar, modificar i afegir productes.

#### OrderedProducts

Aquest controlador s'encarrega de la relació entre les comandes i els productes, aquest és el contrlador que permet als usuaris afegir els productes a la comanda que ja té creada, si s'intenta afegir un producte sense crear una comanda es crearà una comanda, a més de poder modificar els productes que s'afegeixen a la comanda i poder eliminar-los si es vol. Un client també pot veure el seu historial complet de les comandes que ha realitzat.

Els administradors i empleats podrán crear, modificar, eliminar comandes si és necessari. Un client només podrà editar la quantitat de productes que vol en la comanda, un administrador o un empleat podrà editar tot el que està relacionat amb la comanda.

Els administradors també poden veure l'historial compelet de totes les comandes realitzades per a tots els usuaris.

## Pàgina web

La pàgina web és qui es comunica amb la api i per a veure, modificar el que hi ha a la base de dades, a través d'una interfície web.

La pàgina web té diferents vistes, si ets un usuari anònim no veurás gaire més que els productes, si inicies sessió com a client llavors ja podrás realitzar comandes i veure el teu perfil, si inicies com a administrador podrás editar de veritat la base de dades, canviant registres d'aquesta.

La pàgina web per als clients serveix com una forma de demanar els productes de la botiga de forma en línia, en comptes d'assistir presensialment, a més de poder revisar el catàleg de forma molt més ràpida i actualitzada.

Per als empleats serveix per a poder editar i controlar l'estoc dels productes de la base de dades, aixi com poder afegir-ne de nous. També poden veure les estadístiques dels usuaris així com les seves comandes.

La pàgina web conté una traducció automàtica a partir d'un selector d'idioma, a través d'aquest selector és possible canviar l'idioma de la pàgina web a tres possibles opcions, català, idioma per defecte, catsellà i anglès. En canviar el idioma tota la pàgina web canviarà al idioma seleccionat.