namespace NorCommerce.DataAccess.Seed
{
    /// <summary>
    /// One book in the demo catalog.
    /// SearchTitle / SearchAuthor are only used to find a cover on Open Library
    /// (English titles give far more cover hits than Norwegian ones).
    /// </summary>
    public record SeedBook(
        string Title,
        string Author,
        string Category,
        string Description,
        bool Bestseller = false,
        string? SearchTitle = null,
        string? SearchAuthor = null);

    /// <summary>
    /// 80 public-domain books in 8 categories. No copyright issues for a demo store.
    /// Descriptions are original short texts written for this project.
    /// </summary>
    public static class BookCatalog
    {
        public const string NorwegianClassics = "Norske klassikere";
        public const string Crime = "Krim og spenning";
        public const string FolkTales = "Eventyr og sagn";
        public const string WorldClassics = "Verdensklassikere";
        public const string SciFi = "Sci-fi og fantasy";
        public const string Children = "Barn og ungdom";
        public const string Drama = "Drama og poesi";
        public const string NonFiction = "Historie og filosofi";

        public static readonly string[] Categories =
        {
            NorwegianClassics, Crime, FolkTales, WorldClassics,
            SciFi, Children, Drama, NonFiction
        };

        public static readonly SeedBook[] Books =
        {
            // ---------- Norske klassikere ----------
            new("Sult", "Knut Hamsun", NorwegianClassics,
                "<p>En ung forfatter vandrer sulten gjennom Kristiania og kjemper for både brødet og verdigheten. Romanen som gjorde Hamsun kjent.</p>",
                true, "Hunger", "Knut Hamsun"),
            new("Markens grøde", "Knut Hamsun", NorwegianClassics,
                "<p>Isak rydder jord i villmarken og bygger et liv fra ingenting. Romanen ga Hamsun Nobelprisen i litteratur.</p>",
                false, "Growth of the Soil", "Knut Hamsun"),
            new("Pan", "Knut Hamsun", NorwegianClassics,
                "<p>Løytnant Glahn lever i skogen i Nordland og forelsker seg i den uforutsigbare Edvarda. En intens roman om natur og lidenskap.</p>",
                false, "Pan", "Knut Hamsun"),
            new("Victoria", "Knut Hamsun", NorwegianClassics,
                "<p>Møllersønnen Johannes og slottsdatteren Victoria elsker hverandre, men skilles av klasse og stolthet.</p>",
                false, "Victoria", "Knut Hamsun"),
            new("Synnøve Solbakken", "Bjørnstjerne Bjørnson", NorwegianClassics,
                "<p>En bondefortelling om kjærlighet og forsoning mellom to gårder i en norsk dal.</p>",
                false, "Synnove Solbakken", "Bjørnstjerne Bjørnson"),
            new("En glad gutt", "Bjørnstjerne Bjørnson", NorwegianClassics,
                "<p>Øyvind vokser opp med drømmer om skole og kjærlighet, og lærer at glede må arbeides for.</p>",
                false, "A Happy Boy", "Bjørnstjerne Bjørnson"),
            new("Kristin Lavransdatter", "Sigrid Undset", NorwegianClassics,
                "<p>Et mektig middelalderepos om en kvinne som følger hjertet sitt mot farens vilje.</p>",
                true, "Kristin Lavransdatter", "Sigrid Undset"),
            new("Constance Ring", "Amalie Skram", NorwegianClassics,
                "<p>En modig roman om ekteskap, frihet og kvinners stilling i det borgerlige Norge.</p>",
                false, "Constance Ring", "Amalie Skram"),
            new("Garman & Worse", "Alexander L. Kielland", NorwegianClassics,
                "<p>En skarp og morsom skildring av en handelsfamilie i en vestlandsk by, inspirert av Kiellands eget Stavanger.</p>",
                false, "Garman and Worse", "Alexander Kielland"),
            new("Familien paa Gilje", "Jonas Lie", NorwegianClassics,
                "<p>Livet på en embetsgård i Gudbrandsdalen, sett gjennom døtrenes drømmer og valg.</p>",
                false, "The Family at Gilje", "Jonas Lie"),

            // ---------- Krim og spenning ----------
            new("A Study in Scarlet", "Arthur Conan Doyle", Crime,
                "<p>Sherlock Holmes og dr. Watson møtes for første gang og løser et mystisk mord i London.</p>"),
            new("The Sign of Four", "Arthur Conan Doyle", Crime,
                "<p>En skatt, en pakt og en hemmelighetsfull besøkende fører Holmes og Watson ut på Themsen.</p>"),
            new("The Hound of the Baskervilles", "Arthur Conan Doyle", Crime,
                "<p>En sagnomsust hund skremmer slekten Baskerville. Holmes' mest berømte sak.</p>",
                true),
            new("The Adventures of Sherlock Holmes", "Arthur Conan Doyle", Crime,
                "<p>Tolv klassiske noveller med verdens mest kjente detektiv.</p>",
                true),
            new("The Moonstone", "Wilkie Collins", Crime,
                "<p>En hellig diamant forsvinner fra et engelsk landsted. Regnes som en av de første detektivromanene.</p>"),
            new("The Woman in White", "Wilkie Collins", Crime,
                "<p>En mystisk kvinne i hvitt, et arveoppgjør og en sammensvergelse. Spenning fra første side.</p>"),
            new("The Innocence of Father Brown", "G. K. Chesterton", Crime,
                "<p>Den beskjedne presten Father Brown løser kriminalgåter med menneskekunnskap og humor.</p>"),
            new("The Thirty-Nine Steps", "John Buchan", Crime,
                "<p>Richard Hannay blir jaget over de skotske høylandene etter å ha snublet over en spionring.</p>"),
            new("The Mystery of the Yellow Room", "Gaston Leroux", Crime,
                "<p>Et drapsforsøk i et låst rom uten utgang. En klassiker blant umulige mysterier.</p>"),
            new("Arsène Lupin, Gentleman Thief", "Maurice Leblanc", Crime,
                "<p>Den sjarmerende mestertyven Arsène Lupin lurer politiet igjen og igjen.</p>",
                false, "Arsene Lupin, Gentleman-Cambrioleur", "Maurice Leblanc"),

            // ---------- Eventyr og sagn ----------
            new("Norske folkeeventyr", "P. Chr. Asbjørnsen og Jørgen Moe", FolkTales,
                "<p>Askeladden, troll og prinsesser. De kjente norske eventyrene samlet på ett sted.</p>",
                true, "Norwegian Folk Tales", "Asbjørnsen"),
            new("Grimms eventyr", "Brødrene Grimm", FolkTales,
                "<p>Snøhvit, Hans og Grete og Rødhette. Eventyrene som har fulgt barn i over 200 år.</p>",
                false, "Grimms' Fairy Tales", "Grimm"),
            new("H.C. Andersens eventyr", "H.C. Andersen", FolkTales,
                "<p>Den lille havfrue, Den stygge andungen og mange flere tidløse fortellinger.</p>",
                false, "Fairy Tales", "Hans Christian Andersen"),
            new("Snorres kongesagaer", "Snorre Sturlason", FolkTales,
                "<p>Sagaene om de norske kongene, fra mytisk tid til middelalderen.</p>",
                false, "Heimskringla", "Snorri Sturluson"),
            new("Den eldre Edda", "Ukjent", FolkTales,
                "<p>Norrøne dikt om Odin, Tor, Loke og verdens skapelse og undergang.</p>",
                false, "Poetic Edda", null),
            new("Tusen og én natt", "Ukjent", FolkTales,
                "<p>Sjeherasad forteller historier natt etter natt for å redde livet sitt.</p>",
                false, "Arabian Nights", null),
            new("Aesops fabler", "Aisopos", FolkTales,
                "<p>Korte fabler med dyr og en moral, fortalt i over to tusen år.</p>",
                false, "Aesop's Fables", "Aesop"),
            new("Perraults eventyr", "Charles Perrault", FolkTales,
                "<p>Tornerose, Askepott og Mestermus i sine eldste kjente versjoner.</p>",
                false, "Tales of Mother Goose", "Charles Perrault"),
            new("The Blue Fairy Book", "Andrew Lang", FolkTales,
                "<p>En klassisk samling eventyr fra hele verden, valgt ut for både barn og voksne.</p>"),
            new("Beowulf", "Ukjent", FolkTales,
                "<p>Helten Beowulf kjemper mot uhyret Grendel i det eldste store diktet på engelsk.</p>",
                false, "Beowulf", null),

            // ---------- Verdensklassikere ----------
            new("Pride and Prejudice", "Jane Austen", WorldClassics,
                "<p>Elizabeth Bennet og Mr. Darcy må overvinne stolthet og fordommer. En av verdens mest elskede romaner.</p>",
                true),
            new("Great Expectations", "Charles Dickens", WorldClassics,
                "<p>Den fattige gutten Pip får en ukjent velgjører og et nytt liv i London.</p>"),
            new("Anna Karenina", "Leo Tolstoy", WorldClassics,
                "<p>En skjebnesvanger kjærlighetshistorie i det russiske overklassesamfunnet.</p>"),
            new("Crime and Punishment", "Fyodor Dostoevsky", WorldClassics,
                "<p>Studenten Raskolnikov begår et mord og må leve med sin egen samvittighet.</p>",
                true),
            new("Les Misérables", "Victor Hugo", WorldClassics,
                "<p>Den tidligere straffangen Jean Valjean søker en ny start i et urolig Frankrike.</p>"),
            new("Moby-Dick", "Herman Melville", WorldClassics,
                "<p>Kaptein Ahab jakter besatt på den hvite hvalen som tok beinet hans.</p>"),
            new("Jane Eyre", "Charlotte Brontë", WorldClassics,
                "<p>En foreldreløs guvernante finner kjærlighet og hemmeligheter på Thornfield Hall.</p>"),
            new("Wuthering Heights", "Emily Brontë", WorldClassics,
                "<p>Heathcliff og Catherine i en vill og mørk kjærlighetshistorie på de engelske heiene.</p>"),
            new("The Picture of Dorian Gray", "Oscar Wilde", WorldClassics,
                "<p>Dorian forblir ung mens portrettet hans eldes og avslører hvem han egentlig er.</p>"),
            new("Madame Bovary", "Gustave Flaubert", WorldClassics,
                "<p>Emma Bovary drømmer om et liv fylt av romantikk og luksus, men virkeligheten er en annen.</p>"),

            // ---------- Sci-fi og fantasy ----------
            new("Twenty Thousand Leagues Under the Sea", "Jules Verne", SciFi,
                "<p>Kaptein Nemo tar leseren med på reise i ubåten Nautilus.</p>"),
            new("Journey to the Center of the Earth", "Jules Verne", SciFi,
                "<p>En professor og nevøen hans klatrer ned i et islandsk vulkankrater mot jordens indre.</p>"),
            new("Around the World in Eighty Days", "Jules Verne", SciFi,
                "<p>Phileas Fogg vedder hele formuen på at han kan reise jorden rundt på 80 dager.</p>",
                true),
            new("The Time Machine", "H. G. Wells", SciFi,
                "<p>En oppfinner reiser hundretusener av år inn i fremtiden og møter menneskehetens etterkommere.</p>"),
            new("The War of the Worlds", "H. G. Wells", SciFi,
                "<p>Marsboere lander i England, og menneskeheten står maktesløs.</p>"),
            new("The Invisible Man", "H. G. Wells", SciFi,
                "<p>En forsker gjør seg selv usynlig, men oppdager at prisen er høy.</p>"),
            new("Frankenstein", "Mary Shelley", SciFi,
                "<p>Victor Frankenstein skaper liv og må ta ansvar for skapningen sin. Ofte kalt den første science fiction-romanen.</p>",
                true),
            new("Dracula", "Bram Stoker", SciFi,
                "<p>Grev Dracula reiser fra Transilvania til England, og en liten gruppe må stoppe ham.</p>"),
            new("Strange Case of Dr Jekyll and Mr Hyde", "Robert Louis Stevenson", SciFi,
                "<p>En respektert lege slipper løs sin mørke side, med skremmende følger.</p>"),
            new("A Princess of Mars", "Edgar Rice Burroughs", SciFi,
                "<p>John Carter våkner på Mars og havner midt i en krig mellom fremmede folk.</p>"),

            // ---------- Barn og ungdom ----------
            new("Alice's Adventures in Wonderland", "Lewis Carroll", Children,
                "<p>Alice faller ned i et kaninhull og havner i en verden der ingenting er som det skal.</p>",
                true),
            new("Treasure Island", "Robert Louis Stevenson", Children,
                "<p>Unge Jim Hawkins finner et skattekart og seiler ut med sjørøveren Long John Silver.</p>"),
            new("The Adventures of Tom Sawyer", "Mark Twain", Children,
                "<p>Tom Sawyer finner på ugagn og eventyr langs Mississippi-elven.</p>"),
            new("Peter Pan", "J. M. Barrie", Children,
                "<p>Gutten som aldri ville bli voksen tar Wendy og brødrene med til Aldri-Aldri-land.</p>"),
            new("The Jungle Book", "Rudyard Kipling", Children,
                "<p>Mowgli vokser opp blant ulver, bjørnen Baloo og panteren Bagheera.</p>"),
            new("Anne of Green Gables", "L. M. Montgomery", Children,
                "<p>Den fantasifulle og foreldreløse Anne finner et hjem hos to søsken på Prince Edward Island.</p>"),
            new("Heidi", "Johanna Spyri", Children,
                "<p>Heidi flytter til bestefaren i de sveitsiske Alpene og sprer glede rundt seg.</p>"),
            new("Black Beauty", "Anna Sewell", Children,
                "<p>Historien om en hest, fortalt av hesten selv. En tidlig bok om dyrevelferd.</p>"),
            new("The Wonderful Wizard of Oz", "L. Frank Baum", Children,
                "<p>Dorothy blåses til landet Oz og følger den gule mursteinsveien sammen med nye venner.</p>"),
            new("Little Women", "Louisa May Alcott", Children,
                "<p>Fire søstre vokser opp under den amerikanske borgerkrigen, med drømmer, krangler og samhold.</p>"),

            // ---------- Drama og poesi ----------
            new("Et dukkehjem", "Henrik Ibsen", Drama,
                "<p>Nora oppdager at ekteskapet hennes bygger på en løgn, og tar et valg som sjokkerte et helt publikum.</p>",
                true, "A Doll's House", "Henrik Ibsen"),
            new("Peer Gynt", "Henrik Ibsen", Drama,
                "<p>Den fantasifulle løgneren Peer reiser verden rundt på jakt etter seg selv.</p>",
                true, "Peer Gynt", "Henrik Ibsen"),
            new("Gengangere", "Henrik Ibsen", Drama,
                "<p>Fortidens synder kommer tilbake og hjemsøker familien Alving.</p>",
                false, "Ghosts", "Henrik Ibsen"),
            new("Vildanden", "Henrik Ibsen", Drama,
                "<p>Sannheten blir et farlig våpen når Gregers Werle blander seg inn i familien Ekdals liv.</p>",
                false, "The Wild Duck", "Henrik Ibsen"),
            new("Hedda Gabler", "Henrik Ibsen", Drama,
                "<p>En av teaterhistoriens mest gåtefulle kvinneroller, fanget i et liv hun ikke ønsker.</p>",
                false, "Hedda Gabler", "Henrik Ibsen"),
            new("En folkefiende", "Henrik Ibsen", Drama,
                "<p>Doktor Stockmann avslører at byens bad er forurenset, og blir folkets fiende.</p>",
                false, "An Enemy of the People", "Henrik Ibsen"),
            new("Hamlet", "William Shakespeare", Drama,
                "<p>Prinsen av Danmark søker hevn for farens død. Å være eller ikke være.</p>"),
            new("Macbeth", "William Shakespeare", Drama,
                "<p>En profeti driver Macbeth og hans ambisiøse kone mot makt og undergang.</p>"),
            new("Faust", "Johann Wolfgang von Goethe", Drama,
                "<p>Den lærde Faust inngår en pakt med djevelen Mefistofeles for kunnskap og opplevelser.</p>"),
            new("Leaves of Grass", "Walt Whitman", Drama,
                "<p>Diktsamlingen som feiret mennesket, naturen og det moderne Amerika.</p>"),

            // ---------- Historie og filosofi ----------
            new("The Prince", "Niccolò Machiavelli", NonFiction,
                "<p>En klassisk og omstridt guide til makt og politikk.</p>"),
            new("Meditations", "Marcus Aurelius", NonFiction,
                "<p>Den romerske keiserens personlige notater om ro, plikt og et godt liv.</p>",
                true),
            new("The Art of War", "Sun Tzu", NonFiction,
                "<p>Kinesisk strategi som fortsatt leses av ledere, militære og forretningsfolk.</p>"),
            new("The Republic", "Plato", NonFiction,
                "<p>Sokrates diskuterer rettferdighet og den ideelle staten.</p>"),
            new("Thus Spoke Zarathustra", "Friedrich Nietzsche", NonFiction,
                "<p>En filosofisk fortelling om mennesket, verdier og overmennesket.</p>"),
            new("On the Origin of Species", "Charles Darwin", NonFiction,
                "<p>Boken som forklarte evolusjon gjennom naturlig utvalg og endret vitenskapen for alltid.</p>"),
            new("Walden", "Henry David Thoreau", NonFiction,
                "<p>To år i en hytte ved Walden Pond, og tanker om et enkelt og bevisst liv.</p>"),
            new("The Wealth of Nations", "Adam Smith", NonFiction,
                "<p>Grunnlaget for moderne økonomi, om marked, arbeid og handel.</p>"),
            new("Fram over Polhavet", "Fridtjof Nansen", NonFiction,
                "<p>Nansens egen beretning om Fram-ekspedisjonen og ferden mot Nordpolen.</p>",
                false, "Farthest North", "Fridtjof Nansen"),
            new("Sydpolen", "Roald Amundsen", NonFiction,
                "<p>Amundsens beretning om hvordan han og mennene hans nådde Sydpolen først, i 1911.</p>",
                false, "The South Pole", "Roald Amundsen"),
        };
    }
}
