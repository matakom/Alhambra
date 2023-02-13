# Rámcová analýza projektu Alhambra

## Stručná verze projektového záměru
Záměr projektu je předělat karetní hru „Alhambra: the card game“ do hratelné multiplayer verze.

## Rozsah realizace projektu
Při zapnutí hry se hráč přihlásí pomocí mailu. Může si prohlídnout svůj profil s různými statistikami (včetně počtu výher, počtu her, stáří účtu, nejvýše dosaženého skóre). Hráč bude moci vytvořit lobby, nebo se do lobby připojit pomocí šesticiferného kódu. Následně bude moci hru spustit s dalšími hráči připojenými přes multiplayer (až 4 hráči v jedné hře). Hráči se následně střídají ve svých kolech podle pravidel hry. Když hra skončí, tak se vypíše pořadí a skóre hráčů.

## Technologický stack a zdůvodnění výběru daných technologí
Využiji herní engine unity, jelikož umožňuje lehkou práci s grafickými prvky a vstupy hráčů. Herní logika bude na C# serveru, s kterým bude komunikovat klient pomocí HTTP endpointů pro stažení dat na začátku a konci hry. Při hře pak bude komunikace probíhat v rozhraní webSocket. Obě rozhraní budou pracovat s JSON. Posledním prvkem bude mySQL databáze, ve které budou data o aktuálních hrách a seznam všech registrovaných hráčů. Databáze bude komunikovat s hlavním serverem.

## Hrubá časová osa zpracování projektu (milestones)
|Měsíc|Náplň práce|
|-------|-------------|
|Leden|Správa účtu|
|Únor|Systém lobby|
|Březen|Vytvoření obsahu JSON packetů|
|Duben|Reakce na JSON packety|
|Květen|Logika a grafika hry|
|Červen|Testování, doladění|

## Component diagram systému dle notace UML
![component diagram UML](https://github.com/matakom/Alhambra/blob/main/Assets/componentDiagraM.jpeg)
