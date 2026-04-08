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
        var kdgLogo = new Media
        {
            Uri = "/images/logos/kdg-logo.png"
        };
        var apLogo = new Media
        {
            Uri = "/images/logos/AP-logo.png"
        };
        var project = new Project
        {
            Introduction =
                "Jouw welzijn telt. Denk met ons mee.\n\n" +
                "Deze vragenlijst is anoniem en helpt ons een actieplan mentaal welzijn te maken.",
            Status = Status.Active,
            Type = ProjectType.VerticalScroll,
            Prompt = "",
            Duration = 10,
            ReleaseDate = DateTime.UtcNow,
            SubPlatform = subPlatform,
            Logo =kdgLogo,
            Photo = new Media
            {
                Uri = "/images/photos/kdg-Photo.jpg"
            }
        };

        var project2 = new Project
        {
            Introduction =
                "Deze vragenlijst is anoniem en helpt ons inzicht te krijgen in het campusleven.",
            Status = Status.Active,
            Type = ProjectType.VerticalScroll,
            Prompt = "",
            Duration = 30,
            ReleaseDate = DateTime.UtcNow,
            SubPlatform = subPlatform,
            Logo = kdgLogo,
            Photo = new Media
            {
                Uri = "/images/photos/kdg-Photo.jpg"
            }
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
           SUBPLATFORM 2 - AP HOGESCHOOL
        ========================= */

        var apSubPlatform = new SubPlatform
        {
            CompanyName = "AP Hogeschool",
            Slug = "ap-hogeschool",
            Language = Language.Nl,
            Platform = platform
        };

        var apProject1 = new Project
        {
            Introduction =
                "Jouw stem telt bij AP Hogeschool.\n\n" +
                "Met deze bevraging willen we beter begrijpen hoe studenten hun mentaal welzijn ervaren " +
                "en welke ondersteuning volgens hen echt het verschil maakt.",
            Status = Status.Active,
            Type = ProjectType.VerticalScroll,
            Prompt = "",
            Duration = 12,
            ReleaseDate = DateTime.UtcNow,
            SubPlatform = apSubPlatform,
            Logo = apLogo,
            Photo = new Media
            {
                Uri = "/images/photos/ap-photo.jpg"
            }
        };

        var apProject2 = new Project
        {
            Introduction =
                "Hoe maken we van AP een campus waar studenten zich welkom, veilig en verbonden voelen?\n\n" +
                "Via deze bevraging en ideeënronde verzamelen we concrete input van studenten.",
            Status = Status.Active,
            Type = ProjectType.VerticalScroll,
            Prompt = "",
            Duration = 10,
            ReleaseDate = DateTime.UtcNow,
            SubPlatform = apSubPlatform,
            Logo = apLogo,
            Photo = new Media
            {
                Uri = "/images/photos/ap-photo.jpg"
            }
        };

        /* =========================
           AP PROJECT 1
           Mentaal welzijn & ondersteuning
        ========================= */

        var apQuestionList1 = new QuestionList
        {
            Project = apProject1
        };

        var apSection1 = new Section
        {
            Title = "Bevraging (AP mentaal welzijn)",
            Order = 1,
            QuestionList = apQuestionList1,
            Questions = new List<Question>()
        };

        var apQuestions1 = (List<Question>)apSection1.Questions;

        apQuestions1.Add(new Question
        {
            Description = "Hoe ervaar jij je mentaal welzijn tijdens het academiejaar?",
            QuestionType = QuestionType.SingleChoice,
            Section = apSection1,
            Options = new List<QuestionOption>
            {
                new() { Text = "Zeer goed" },
                new() { Text = "Goed" },
                new() { Text = "Wisselend" },
                new() { Text = "Eerder moeilijk" },
                new() { Text = "Heel moeilijk" }
            }
        });

        apQuestions1.Add(new Question
        {
            Description = "Welke factoren hebben momenteel de grootste impact op jouw welzijn?",
            QuestionType = QuestionType.MultipleChoice,
            Section = apSection1,
            Options = new List<QuestionOption>
            {
                new() { Text = "Studiedruk en deadlines" },
                new() { Text = "Combinatie studie en werk" },
                new() { Text = "Financiële zorgen" },
                new() { Text = "Eenzaamheid of sociale druk" },
                new() { Text = "Gezondheid of vermoeidheid" },
                new() { Text = "Thuissituatie" },
                new() { Text = "Onzekerheid over de toekomst" },
                new() { Text = "Andere" }
            }
        });

        apQuestions1.Add(new Question
        {
            Description = "In welke mate voel jij je ondersteund door AP wanneer het moeilijk gaat?",
            QuestionType = QuestionType.Range,
            Section = apSection1,
            RangeMin = 1,
            RangeMax = 10,
            RangeMinLabel = "1 = helemaal niet ondersteund",
            RangeMaxLabel = "10 = zeer goed ondersteund"
        });

        apQuestions1.Add(new Question
        {
            Description = "Weet jij waar je binnen AP terechtkan voor hulp of begeleiding?",
            QuestionType = QuestionType.SingleChoice,
            Section = apSection1,
            Options = new List<QuestionOption>
            {
                new() { Text = "Ja, en ik heb al hulp gezocht" },
                new() { Text = "Ja, maar ik heb nog geen hulp gezocht" },
                new() { Text = "Nee, dat weet ik niet" }
            }
        });

        apQuestions1.Add(new Question
        {
            Description = "Wat zou AP volgens jou concreet kunnen verbeteren om studenten beter mentaal te ondersteunen?",
            QuestionType = QuestionType.OpenQuestion,
            Section = apSection1
        });

        apQuestionList1.Sections = new List<Section> { apSection1 };
        apProject1.QuestionList = apQuestionList1;

        var apTopic1 = new Topic { Theme = "Studiedruk & planning", Project = apProject1 };
        var apTopic2 = new Topic { Theme = "Hulp en begeleiding", Project = apProject1 };
        var apTopic3 = new Topic { Theme = "Welzijnsacties op campus", Project = apProject1 };
        var apTopic4 = new Topic { Theme = "Communicatie naar studenten", Project = apProject1 };

        apProject1.Topics = new List<Topic>
        {
            apTopic1, apTopic2, apTopic3, apTopic4
        };

        var apIdea1 = new Idea
        {
            Title = "Meer spreiding van deadlines",
            Text = "Veel stress ontstaat wanneer verschillende opdrachten en examens in dezelfde periode samenvallen. Een betere spreiding zou helpen.",
            Topic = apTopic1,
            ModerationStatus = ModerationStatus.Accepted
        };

        var apIdea2 = new Idea
        {
            Title = "Snellere toegang tot studentenbegeleiding",
            Text = "Het zou fijn zijn als studenten sneller een eerste gesprek kunnen krijgen wanneer ze zich mentaal niet goed voelen.",
            Topic = apTopic2,
            ModerationStatus = ModerationStatus.Accepted
        };

        var apIdea3 = new Idea
        {
            Title = "Rustplekken op campus",
            Text = "Voorzie stille of rustige plekken waar studenten even kunnen ontprikkelen tussen de lessen door.",
            Topic = apTopic3,
            ModerationStatus = ModerationStatus.Accepted
        };

        var apIdea4 = new Idea
        {
            Title = "Meer zichtbare communicatie over hulp",
            Text = "Veel studenten weten niet waar ze terechtkunnen. Regelmatige communicatie via Toledo, mail en schermen op campus zou helpen.",
            Topic = apTopic4,
            ModerationStatus = ModerationStatus.Accepted
        };

        var apReaction1 = new Reaction
        {
            Text = "Helemaal akkoord, vooral in de examenperiode is de werkdruk echt te hoog.",
            Idea = apIdea1,
            ModerationStatus = ModerationStatus.Accepted
        };

        var apReaction2 = new Reaction
        {
            Text = "Een eerste gesprek sneller kunnen boeken zou voor veel studenten al een groot verschil maken.",
            Idea = apIdea2,
            ModerationStatus = ModerationStatus.Accepted
        };

        var apReaction3 = new Reaction
        {
            Text = "Ja, zeker op drukke campussen is er nood aan een rustige ruimte.",
            Idea = apIdea3,
            ModerationStatus = ModerationStatus.Accepted
        };

        apIdea1.Reactions = new List<Reaction> { apReaction1 };
        apIdea2.Reactions = new List<Reaction> { apReaction2 };
        apIdea3.Reactions = new List<Reaction> { apReaction3 };
        apIdea4.Reactions = new List<Reaction>();

        var apIdeas1 = new List<Idea> { apIdea1, apIdea2, apIdea3, apIdea4 };
        var apReactions1 = new List<Reaction> { apReaction1, apReaction2, apReaction3 };

        /* =========================
           AP PROJECT 2
           Campusbeleving & verbondenheid
        ========================= */

        var apQuestionList2 = new QuestionList
        {
            Project = apProject2
        };

        var apSection2 = new Section
        {
            Title = "Bevraging (AP campusbeleving)",
            Order = 1,
            QuestionList = apQuestionList2,
            Questions = new List<Question>()
        };

        var apQuestions2 = (List<Question>)apSection2.Questions;

        apQuestions2.Add(new Question
        {
            Description = "Hoe welkom voel jij je op jouw campus of opleiding?",
            QuestionType = QuestionType.SingleChoice,
            Section = apSection2,
            Options = new List<QuestionOption>
            {
                new() { Text = "Heel welkom" },
                new() { Text = "Eerder welkom" },
                new() { Text = "Neutraal" },
                new() { Text = "Eerder niet welkom" },
                new() { Text = "Helemaal niet welkom" }
            }
        });

        apQuestions2.Add(new Question
        {
            Description = "Wat helpt jou het meest om je verbonden te voelen met AP?",
            QuestionType = QuestionType.MultipleChoice,
            Section = apSection2,
            Options = new List<QuestionOption>
            {
                new() { Text = "Goede sfeer in de klas" },
                new() { Text = "Activiteiten op campus" },
                new() { Text = "Ondersteunende docenten" },
                new() { Text = "Vrienden en medestudenten" },
                new() { Text = "Studentenverenigingen" },
                new() { Text = "Digitale communicatie" },
                new() { Text = "Een aangename campusomgeving" },
                new() { Text = "Andere" }
            }
        });

        apQuestions2.Add(new Question
        {
            Description = "In welke mate voel jij je verbonden met andere studenten?",
            QuestionType = QuestionType.Range,
            Section = apSection2,
            RangeMin = 1,
            RangeMax = 5,
            RangeMinLabel = "1 = helemaal niet verbonden",
            RangeMaxLabel = "5 = sterk verbonden"
        });

        apQuestions2.Add(new Question
        {
            Description = "Heb jij het gevoel dat AP voldoende initiatieven organiseert om studenten samen te brengen?",
            QuestionType = QuestionType.SingleChoice,
            Section = apSection2,
            Options = new List<QuestionOption>
            {
                new() { Text = "Ja, zeker" },
                new() { Text = "Eerder wel" },
                new() { Text = "Neutraal" },
                new() { Text = "Eerder niet" },
                new() { Text = "Nee" }
            }
        });

        apQuestions2.Add(new Question
        {
            Description = "Welke concrete ideeën heb jij om de campusbeleving bij AP te verbeteren?",
            QuestionType = QuestionType.OpenQuestion,
            Section = apSection2
        });

        apQuestionList2.Sections = new List<Section> { apSection2 };
        apProject2.QuestionList = apQuestionList2;

        var apTopic21 = new Topic { Theme = "Campusleven", Project = apProject2 };
        var apTopic22 = new Topic { Theme = "Ontmoeting & verbondenheid", Project = apProject2 };
        var apTopic23 = new Topic { Theme = "Studentenactiviteiten", Project = apProject2 };
        var apTopic24 = new Topic { Theme = "Campusinfrastructuur", Project = apProject2 };

        apProject2.Topics = new List<Topic>
        {
            apTopic21, apTopic22, apTopic23, apTopic24
        };

        var apIdea21 = new Idea
        {
            Title = "Meer laagdrempelige middagactiviteiten",
            Text = "Korte activiteiten tijdens de middagpauze kunnen studenten makkelijker samenbrengen zonder grote tijdsinvestering.",
            Topic = apTopic23,
            ModerationStatus = ModerationStatus.Accepted
        };

        var apIdea22 = new Idea
        {
            Title = "Een gezellige ontmoetingsruimte",
            Text = "Een vaste plek met zitruimte en rustige sfeer zou studenten helpen om elkaar spontaan te ontmoeten.",
            Topic = apTopic24,
            ModerationStatus = ModerationStatus.Accepted
        };

        var apIdea23 = new Idea
        {
            Title = "Buddywerking voor eerstejaars",
            Text = "Nieuwe studenten voelen zich sneller thuis wanneer ze gekoppeld worden aan een oudere student.",
            Topic = apTopic22,
            ModerationStatus = ModerationStatus.Accepted
        };

        var apIdea24 = new Idea
        {
            Title = "Meer communicatie over wat er al bestaat",
            Text = "Soms zijn er al leuke initiatieven, maar veel studenten weten gewoon niet dat ze bestaan.",
            Topic = apTopic21,
            ModerationStatus = ModerationStatus.Accepted
        };

        var apReaction21 = new Reaction
        {
            Text = "Dat zou echt helpen, zeker voor pendelstudenten die minder snel aansluiting vinden.",
            Idea = apIdea21,
            ModerationStatus = ModerationStatus.Accepted
        };

        var apReaction22 = new Reaction
        {
            Text = "Een buddywerking zou de start voor nieuwe studenten minder overweldigend maken.",
            Idea = apIdea23,
            ModerationStatus = ModerationStatus.Accepted
        };

        apIdea21.Reactions = new List<Reaction> { apReaction21 };
        apIdea22.Reactions = new List<Reaction>();
        apIdea23.Reactions = new List<Reaction> { apReaction22 };
        apIdea24.Reactions = new List<Reaction>();

        var apIdeas2 = new List<Idea> { apIdea21, apIdea22, apIdea23, apIdea24 };
        var apReactions2 = new List<Reaction> { apReaction21, apReaction22 };

        
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

  
        
        /* =========================
           DATABASE INSERT - AP
        ========================= */

        dbContext.SubPlatforms.Add(apSubPlatform);

        dbContext.Projects.Add(apProject1);
        dbContext.Projects.Add(apProject2);

        dbContext.QuestionList.Add(apQuestionList1);
        dbContext.Section.Add(apSection1);
        dbContext.Questions.AddRange(apQuestions1);

        dbContext.QuestionList.Add(apQuestionList2);
        dbContext.Section.Add(apSection2);
        dbContext.Questions.AddRange(apQuestions2);

        dbContext.Topics.AddRange(apProject1.Topics);
        dbContext.Ideas.AddRange(apIdeas1);
        dbContext.Reactions.AddRange(apReactions1);

        dbContext.Topics.AddRange(apProject2.Topics);
        dbContext.Ideas.AddRange(apIdeas2);
        dbContext.Reactions.AddRange(apReactions2);
        
        dbContext.SaveChanges();
        
    }
}