using IntergratieProject.Domain.project;
using IntergratieProject.Domain.Questions;
using IntergratieProject.Domain.ideas;

namespace IntergratieProject.DAL.Ef;

public class DataSeeder
{
    public static void Seed(TreeDbContext dbContext)
    {
        var platform = new Platform
        {
            CompanyName = "Tree Platform"
        };

        var subPlatform = new SubPlatform
        {
            CompanyName = "KdG Hogeschool (Demo)",
            Slug = "kdg-hogeschool",
            Language = Language.Nl,
            Platform = platform
        };

        var project = new Project
        {
            Id = 1,
            Introduction =
                "Jouw welzijn telt. Denk met ons mee.\n\n" +
                "Deze vragenlijst is anoniem en helpt ons een actieplan mentaal welzijn te maken.",
            Status = Status.Active,
            Type = ProjectType.VerticalScroll,
            Prompt = "",
            Duration = 10,
            ReleaseDate = DateTime.UtcNow,
            SubPlatform = subPlatform
        };

        var project2 = new Project
        {
            Id = 2,
            Introduction =
                "Deze vragenlijst is anoniem en helpt ons inzicht te krijgen in het campusleven.",
            Status = Status.Active,
            Type = ProjectType.VerticalScroll,
            Prompt = "",
            Duration = 30,
            ReleaseDate = DateTime.UtcNow,
            SubPlatform = subPlatform
        };

        /* =========================
           PROJECT 1
        ========================= */

        var questionList = new QuestionList
        {
            Project = project
        };

        var section = new Section
        {
            Title = "Bevraging (mentaal welzijn)",
            Order = 1,
            QuestionList = questionList,
            Questions = new List<Question>()
        };

        var questions = (List<Question>)section.Questions;

        questions.Add(new Question
        {
            Description = "Hoe zou jij je mentaal welzijn in de voorbije maand omschrijven?",
            QuestionType = QuestionType.SingleChoice,
            Section = section,
            Options = new List<QuestionOption>
            {
                new() { Text = "Zeer goed" },
                new() { Text = "Goed" },
                new() { Text = "Neutraal" },
                new() { Text = "Eerder slecht" },
                new() { Text = "Zeer slecht" }
            }
        });

        questions.Add(new Question
        {
            Description = "Wat zijn voor jou momenteel de grootste bronnen van stress?",
            QuestionType = QuestionType.MultipleChoice,
            Section = section,
            Options = new List<QuestionOption>
            {
                new() { Text = "Studies / examens" },
                new() { Text = "Combinatie studie-werk" },
                new() { Text = "Financiële zorgen" },
                new() { Text = "Sociale druk / eenzaamheid / relaties" },
                new() { Text = "Fysieke of mentale gezondheid" },
                new() { Text = "Thuissituatie" },
                new() { Text = "Toekomstzorgen" },
                new() { Text = "Andere" }
            }
        });

        questions.Add(new Question
        {
            Description = "In welke mate ervaar jij stress door je studies?",
            QuestionType = QuestionType.Range,
            Section = section,
            RangeMin = 1,
            RangeMax = 5,
            RangeMinLabel = "1 = geen stress",
            RangeMaxLabel = "5 = extreem veel stress"
        });

        questions.Add(new Question
        {
            Description = "Heb je het gevoel dat je de mentale druk die je ervaart meestal aankan?",
            QuestionType = QuestionType.SingleChoice,
            Section = section,
            Options = new List<QuestionOption>
            {
                new() { Text = "Ja, meestal wel" },
                new() { Text = "Soms wel, soms niet" },
                new() { Text = "Eerder niet" },
                new() { Text = "Helemaal niet" }
            }
        });

        questions.Add(new Question
        {
            Description = "Wat doe jij meestal wanneer je het mentaal moeilijk hebt?",
            QuestionType = QuestionType.MultipleChoice,
            Section = section,
            Options = new List<QuestionOption>
            {
                new() { Text = "Ik weet niet goed wat ik kan of moet doen" },
                new() { Text = "Erover praten met ouders/familie/partner" },
                new() { Text = "Erover praten met vrienden/medestudenten" },
                new() { Text = "Professionele hulp zoeken" },
                new() { Text = "Sporten/bewegen" },
                new() { Text = "Afleiding zoeken" },
                new() { Text = "Studietaken uitstellen/vermijden" },
                new() { Text = "Bewust rust inplannen" },
                new() { Text = "Medicatie/alcohol/andere middelen" },
                new() { Text = "Iets anders" }
            }
        });

        questionList.Sections = new List<Section> { section };
        project.QuestionList = questionList;

        var topicStress = new Topic { Theme = "Stress & studies", Project = project };
        var topicSupport = new Topic { Theme = "Ondersteuning hogeschool", Project = project };
        var topicActions = new Topic { Theme = "Ideeën voor acties (ideation)", Project = project };
        var topicDrugs = new Topic { Theme = "Het gebruik van drugs op school", Project = project };

        project.Topics = new List<Topic>
        {
            topicStress, topicSupport, topicActions, topicDrugs
        };

        var idea1 = new Idea
        {
            Title = "Spreid deadlines beter",
            Text = "Veel stress komt doordat meerdere deadlines in dezelfde week vallen.",
            Topic = topicStress,
            ModerationStatus = ModerationStatus.Accepted
        };

        var idea2 = new Idea
        {
            Title = "Extra stille blokruimtes",
            Text = "Tijdens examens zijn er te weinig rustige studieplaatsen.",
            Topic = topicStress,
            ModerationStatus = ModerationStatus.Accepted
        };

        var idea3 = new Idea
        {
            Title = "Meer zichtbaarheid van hulpdiensten",
            Text = "Veel studenten weten niet waar ze terecht kunnen bij mentale problemen.",
            Topic = topicSupport,
            ModerationStatus = ModerationStatus.Accepted
        };

        var idea4 = new Idea
        {
            Title = "Mentale welzijnsweek",
            Text = "Een week met workshops rond stress, slaap en planning.",
            Topic = topicActions,
            ModerationStatus = ModerationStatus.Accepted
        };

        var idea5 = new Idea
        {
            Title = "Betere preventie rond middelengebruik",
            Text = "Meer informatie en begeleiding rond alcohol en drugs.",
            Topic = topicDrugs,
            ModerationStatus = ModerationStatus.Accepted
        };

        var reaction1 = new Reaction
        {
            Text = "Dit zou echt helpen, zeker in de examenperiode wanneer alles samenkomt.",
            Idea = idea1,
            ModerationStatus = ModerationStatus.Accepted
        };

        var reaction2 = new Reaction
        {
            Text = "Mee eens. Vooral groepswerken en individuele deadlines zitten nu vaak te dicht op elkaar.",
            Idea = idea1,
            ModerationStatus = ModerationStatus.Accepted
        };

        var reaction3 = new Reaction
        {
            Text = "Goede suggestie. Ik vind ook dat stille ruimtes vaak te snel volzet zijn.",
            Idea = idea2,
            ModerationStatus = ModerationStatus.Accepted
        };

        var reaction4 = new Reaction
        {
            Text = "Ik wist eerlijk gezegd ook niet goed waar ik terechtkon, dus een centrale pagina lijkt me nuttig.",
            Idea = idea3,
            ModerationStatus = ModerationStatus.Accepted
        };

        idea1.Reactions = new List<Reaction> { reaction1, reaction2 };
        idea2.Reactions = new List<Reaction> { reaction3 };
        idea3.Reactions = new List<Reaction> { reaction4 };
        idea4.Reactions = new List<Reaction>();
        idea5.Reactions = new List<Reaction>();

        var ideas = new List<Idea> { idea1, idea2, idea3, idea4, idea5 };
        var reactions = new List<Reaction> { reaction1, reaction2, reaction3, reaction4 };

        /* =========================
           PROJECT 2
        ========================= */

        var questionList2 = new QuestionList
        {
            Project = project2
        };

        var section2 = new Section
        {
            Title = "Bevraging (campusleven)",
            Order = 1,
            QuestionList = questionList2,
            Questions = new List<Question>()
        };

        var questions2 = (List<Question>)section2.Questions;

        questions2.Add(new Question
        {
            Description = "Hoe ervaar jij het sociale leven op de hogeschool?",
            QuestionType = QuestionType.SingleChoice,
            Section = section2,
            Options = new List<QuestionOption>
            {
                new() { Text = "Heel positief" },
                new() { Text = "Positief" },
                new() { Text = "Neutraal" },
                new() { Text = "Eerder negatief" },
                new() { Text = "Heel negatief" }
            }
        });

        questions2.Add(new Question
        {
            Description = "Welke activiteiten op campus vind jij het meest waardevol?",
            QuestionType = QuestionType.MultipleChoice,
            Section = section2,
            Options = new List<QuestionOption>
            {
                new() { Text = "Workshops" },
                new() { Text = "Studentenevents" },
                new() { Text = "Sportactiviteiten" },
                new() { Text = "Cultuuractiviteiten" },
                new() { Text = "Groepswerken / studie-initiatieven" },
                new() { Text = "Ontspanningsruimtes" },
                new() { Text = "Buddy- of mentorprogramma's" },
                new() { Text = "Andere" }
            }
        });

        questions2.Add(new Question
        {
            Description = "In welke mate voel jij je verbonden met andere studenten?",
            QuestionType = QuestionType.Range,
            Section = section2,
            RangeMin = 1,
            RangeMax = 5,
            RangeMinLabel = "1 = helemaal niet verbonden",
            RangeMaxLabel = "5 = zeer sterk verbonden"
        });

        questions2.Add(new Question
        {
            Description = "Heb je genoeg mogelijkheden om nieuwe mensen te leren kennen?",
            QuestionType = QuestionType.SingleChoice,
            Section = section2,
            Options = new List<QuestionOption>
            {
                new() { Text = "Ja, zeker" },
                new() { Text = "Eerder wel" },
                new() { Text = "Neutraal" },
                new() { Text = "Eerder niet" },
                new() { Text = "Helemaal niet" }
            }
        });

        questions2.Add(new Question
        {
            Description = "Welke initiatieven zouden het campusleven verbeteren?",
            QuestionType = QuestionType.OpenQuestion,
            Section = section2
        });

        questionList2.Sections = new List<Section> { section2 };
        project2.QuestionList = questionList2;

        var topicCampus = new Topic { Theme = "Campusleven & sfeer", Project = project2 };
        var topicEvents = new Topic { Theme = "Studentenevents", Project = project2 };
        var topicConnection = new Topic { Theme = "Verbondenheid tussen studenten", Project = project2 };
        var topicFacilities = new Topic { Theme = "Ontspanningsruimtes", Project = project2 };

        project2.Topics = new List<Topic>
        {
            topicCampus, topicEvents, topicConnection, topicFacilities
        };

        var idea21 = new Idea
        {
            Title = "Lunchactiviteiten voor studenten",
            Text = "Korte activiteiten tijdens de middagpauze om studenten met elkaar te laten connecteren.",
            Topic = topicEvents,
            ModerationStatus = ModerationStatus.Accepted
        };

        var idea22 = new Idea
        {
            Title = "Chillruimte op campus",
            Text = "Een rustige ruimte met zetels waar studenten even kunnen ontspannen.",
            Topic = topicFacilities,
            ModerationStatus = ModerationStatus.Accepted
        };

        var idea23 = new Idea
        {
            Title = "Buddy systeem voor nieuwe studenten",
            Text = "Nieuwe studenten koppelen aan oudere studenten om sneller hun weg te vinden.",
            Topic = topicConnection,
            ModerationStatus = ModerationStatus.Accepted
        };

        var idea24 = new Idea
        {
            Title = "Meer culturele evenementen",
            Text = "Avonden waar studenten hun cultuur kunnen delen met anderen.",
            Topic = topicCampus,
            ModerationStatus = ModerationStatus.Accepted
        };

        var reaction21 = new Reaction
        {
            Text = "Dat zou helpen om sneller nieuwe mensen te leren kennen.",
            Idea = idea21,
            ModerationStatus = ModerationStatus.Accepted
        };

        var reaction22 = new Reaction
        {
            Text = "Een chillruimte zou echt welkom zijn tussen de lessen.",
            Idea = idea22,
            ModerationStatus = ModerationStatus.Accepted
        };

        idea21.Reactions = new List<Reaction> { reaction21 };
        idea22.Reactions = new List<Reaction> { reaction22 };
        idea23.Reactions = new List<Reaction>();
        idea24.Reactions = new List<Reaction>();

        var ideas2 = new List<Idea> { idea21, idea22, idea23, idea24 };
        var reactions2 = new List<Reaction> { reaction21, reaction22 };

        /* =========================
           DATABASE INSERT
        ========================= */

        dbContext.Platforms.Add(platform);
        dbContext.SubPlatforms.Add(subPlatform);
        dbContext.Projects.Add(project);
        dbContext.Projects.Add(project2);

        dbContext.QuestionList.Add(questionList);
        dbContext.Section.Add(section);
        dbContext.Questions.AddRange(questions);

        dbContext.QuestionList.Add(questionList2);
        dbContext.Section.Add(section2);
        dbContext.Questions.AddRange(questions2);

        dbContext.Topics.AddRange(project.Topics);
        dbContext.Ideas.AddRange(ideas);
        dbContext.Reactions.AddRange(reactions);

        dbContext.Topics.AddRange(project2.Topics);
        dbContext.Ideas.AddRange(ideas2);
        dbContext.Reactions.AddRange(reactions2);

        dbContext.SaveChanges();
    }
}