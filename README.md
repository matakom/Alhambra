# Alhambra

## Cíl
Záměr projektu je předělat karetní hru „Alhambra: the card game“ do hratelné multiplayer verze.

## Pravidla hry
Alhambra je logická hra pro 2 - 4 hráče. Jedna hra trvá přibližně 45 minut. Cíl hry je získat největší počet bodů ze všech hráčů.

### Příprava hry
Ve hře se vyskytuje 54 karet s budovami, 108 peněžních karet a 2 bodovací karty (A a B). Do prostředku herní plochy se rozmístí 4 odkryté karty budov, balíček budov, 4 odkryté peněžní karty a balíček peněžních karet, ve kterém je ve 2/5 zamíchaná bodovací karta A a v 4/5 zamíchaná bodovací karta B. Hráči si začnou postupně brát peněžní karty z balíčky, dokud jejich hodnota není alespoň 20. Hru začíná hráč s nejmenším počtem karet, případně ten, s menší hodnotou karet.

### Peněžní karty
Peněžní karty mají různé barvy. Existují modré, žluté, zelené a oranžové. Každá peněžní karta má svojí hodnotu (1-9). Každá kombinace barvy a hodnoty se v balíčku nachází třikrát.

### Tah hráče
Hráč má ve svém tahu na výběr ze dvou možností. Může si vzít peníze, nebo koupit budovu.

#### Braní peněz
Hráč si vezme kteroukoliv z vyložených peněžních karet. Může si jich vzít i více, jejich hodnota však v takovém případě nesmí být větší než 5. Po dokončení tahu hráče, se doplní výběr peněz z balíčku.

#### Koupě budovy
Hráč musí zaplatit daným typem měny určeným na kartičce budovy. Musí použít kombinaci karet, která dává dohromady hodnotu, za kterou se dá budova koupit. Přebytečné peníze se hráči nevrací. Jestliže hráč koupí budovu přesně za cenu, která je na budově napsaná, tak získává další tah a může si opět vybrat z braní peněz a koupí budovy. Po dokončení tahu hráče, se doplní trh peněz z balíčku.

### Konec hry
Hra končí, když jeden z hráčů zakoupí budovu a v balíčku budov už není dostatek karet na doplnění dalších karet budov. V tom momentě se zbývající budovy rozdělí hráčům s největší hodnotou peněz dané barvy. Cena budov v tomto případě nehraje roli. Jestliže má více hráčů stejný počet peněz, tak budovy nezíská nikdo.

### Bodování
Ve hře proběhnou celkem tři kola bodování. Při bodování se řídíme pomocnou tabulkou níže.

#### Bodování A
Bodování nastává při odkrytí bodovací karty A. Body získává pouze hráč, který má nejvíce budov určité rarity.

#### Bodování B
Bodování nastává při odkrytí bodovací karty B. Body získává hráč s největším počtem dané rarity budov a hráč, který má jako druhý nejvíce budov této rarity.

#### Bodování C
Bodování nastává na konci hry. Body získávají 3 hráči, co mají nejvíce bodů.

#### Stejný počet budov
Je možné, že dva nebo více hráčů bude mít stejný počet budov určité rarity. V tomto případě si hráči body rozdělí mezi sebe.


![Tabulka skóre](https://github.com/matakom/Alhambra/blob/main/Assets/kartaSkóre.jpg)

## Možnosti hráče
- Vytvoření účtu
    - Username
    - E-mail
    - Heslo
- Zobrazení profilu se statistikami
    - Počet výher
    - Počet her
    - Stáří účtu
    - Největší dosažený počet bodů
- Lobby
    - Vytvoření
    - Připojení (Šesticiferný kód)
    - Až 4 hráči
- Hraní hry
    - Braní peněz
    - Kupování budov
    - Výpis bodů

<!--- 
# BODY Z TEXTU

Při zapnutí hry se hráč přihlásí pomocí mailu. Může si prohlídnout svůj profil s různými statistikami (včetně počtu výher, počtu her, stáří účtu, nejvýše dosaženého skóre). Hráč bude moci vytvořit lobby, nebo se do lobby připojit pomocí šesticiferného kódu. Následně bude moci hru spustit s dalšími hráči připojenými přes multiplayer (až 4 hráči v jedné hře). Hráči se následně střídají ve svých kolech podle pravidel hry. Když hra skončí, tak se vypíše pořadí a skóre hráčů.
-->

## Technologie

### Klient (Unity)
- C#
- Lehká grafika
- Komunikace se serverem

### Server
- C#
- Komunikace s databází a uživateli

<!--- 

Využiji herní engine unity, jelikož umožňuje lehkou práci s grafickými prvky a vstupy hráčů. Herní logika bude na C# serveru, s kterým bude komunikovat klient pomocí HTTP endpointů pro stažení dat na začátku a konci hry. Při hře pak bude komunikace probíhat v rozhraní webSocket. Obě rozhraní budou pracovat s JSON. Posledním prvkem bude mySQL databáze, ve které budou data o aktuálních hrách a seznam všech registrovaných hráčů. Databáze bude komunikovat s hlavním serverem.
-->



### Databáze
Databáze bude komunikovat pouze se serverem. Použiji MySQL databázi, která bude mít následující tabulky a sloupce.

#### Uživatelé
- ID
- Přezdívka
- E-Mail
- Hash hesla
- Poslední přihlášení
- Ověřený e-mail

#### Hry
- ID hry
- Stav hry
- Délka hry
- Délka probíhajícího tahu
- Počet hráčů
- Hráči
    - Peněžní karty
    - Karty budov
    - Body
- Karty vyložené na stole
    - Peněžní karty
    - Karty budov
- Hráč na tahu

## Nasazení
Hra bude připravena pro nasazení na server. Nasazena však oficiálně nebude, kvůli právům ke hře.

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
