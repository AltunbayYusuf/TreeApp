using IntegratieProject.BL.Domain.Ai;
using IntegratieProject.BL.Domain.ideas;
using IntegratieProject.BL.Domain.project;
using IntegratieProject.BL.Domain.questions;
using IntegratieProject.BL.Domain.Questions;
using IntegratieProject.BL.Domain.users;

namespace IntegratieProject.DAL.Ef;

public class DataSeeder
{
    public static void Seed(TreeDbContext dbContext, string adminIdentityId)
    {
        var platform = new Platform
        {
            CompanyName = "Tree Platform"
        };
      
        
        var kdgLogo = new Media
        {
            Uri = "/images/logos/kdg-logo.png"
        };
        var apLogo = new Media
        {
            Uri = "/images/logos/AP-logo.png"
        };
 
        var generalAdmin = new GeneralAdmin
        {
            Name = "Main Admin",
            IdentityUserId = adminIdentityId,
            Platform = platform
        };
        var kdgAdmin = new SubAdmin
        {
            Name = "kdg",
            GeneralAdmin = generalAdmin
        };
        var apAdmin = new SubAdmin
        {
            Name = "ap",
            GeneralAdmin = generalAdmin
        };
        ICollection<SubAdmin> kdgAdmins = new List<SubAdmin> { kdgAdmin };
        var subPlatform = new SubPlatform
        {
            CompanyName = "KdG Hogeschool (Demo)",
            Slug = "kdg-hogeschool",
            Language = Language.Nl,
            Platform = platform,
            SubAdmins = kdgAdmins
        };

        var project = new Project
        {
            Name = "Actieplan Mentaal Welzijn 2026",
            Introduction =
                "Jouw welzijn telt. Denk met ons mee.\n\n" +
                "Deze vragenlijst is anoniem en helpt ons een actieplan mentaal welzijn te maken.",
            Status = Status.Active,
            Type = ProjectType.VerticalScroll,
            Prompt = "",
            Duration = 10,
            ReleaseDate = DateTime.UtcNow,
            SubPlatform = subPlatform,
            Logo = kdgLogo,
            Photo = new Media
            {
                Uri = "/images/photos/kdg-Photo.jpg"
            }
        };

        var project2 = new Project
        {
            Name = "Campusbeleving & Studentenervaring",
            Introduction =
                "Deze vragenlijst is anoniem en helpt ons inzicht te krijgen in het campusleven.",
            Status = Status.Active,
            Type = ProjectType.Chat,
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

        var mentalWellbeingQuestion = new Question
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
        };

        questions.Add(mentalWellbeingQuestion);

        var stressSourcesQuestion = new Question
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
        };

        questions.Add(stressSourcesQuestion);

        var studyStressQuestion = new Question
        {
            Description = "In welke mate ervaar jij stress door je studies?",
            QuestionType = QuestionType.Range,
            Section = section,
            RangeMin = 1,
            RangeMax = 5,
            RangeMinLabel = "1 = geen stress",
            RangeMaxLabel = "5 = extreem veel stress",
            ConditionalQuestions = new List<ConditionalQuestion>()
        };

        questions.Add(studyStressQuestion);

        var stressFollowUpQuestion = new Question
        {
            Description =
                "Je gaf aan veel stress te ervaren. Wat zou jou concreet helpen om die stress te verminderen?",
            QuestionType = QuestionType.OpenQuestion,
            Section = section,
            IsRequired = true
        };

        questions.Add(stressFollowUpQuestion);

        studyStressQuestion.ConditionalQuestions.Add(new ConditionalQuestion
        {
            ParentQuestion = studyStressQuestion,
            FollowUpQuestion = stressFollowUpQuestion,
            TriggerType = TriggerType.GreaterOrEqual,
            TriggerValue = "4"
        });

        var pressureQuestion = new Question
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
        };

        questions.Add(pressureQuestion);

        var copingQuestion = new Question
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
        };

        questions.Add(copingQuestion);

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

        ICollection<SubAdmin> apAdmins =  new List<SubAdmin> { apAdmin };
        var apSubPlatform = new SubPlatform
        {
            CompanyName = "AP Hogeschool",
            Slug = "ap-hogeschool",
            Language = Language.Nl,
            Platform = platform,
            SubAdmins = apAdmins
        };

        var apProject1 = new Project
        {
            Name = "AP Actieplan Mentaal Welzijn 2026",
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
            Name = "AP Campusbeleving & Verbondenheid",
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
            Description =
                "Wat zou AP volgens jou concreet kunnen verbeteren om studenten beter mentaal te ondersteunen?",
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
            Text =
                "Veel stress ontstaat wanneer verschillende opdrachten en examens in dezelfde periode samenvallen. Een betere spreiding zou helpen.",
            Topic = apTopic1,
            ModerationStatus = ModerationStatus.Accepted
        };

        var apIdea2 = new Idea
        {
            Title = "Snellere toegang tot studentenbegeleiding",
            Text =
                "Het zou fijn zijn als studenten sneller een eerste gesprek kunnen krijgen wanneer ze zich mentaal niet goed voelen.",
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
            Text =
                "Veel studenten weten niet waar ze terechtkunnen. Regelmatige communicatie via Toledo, mail en schermen op campus zou helpen.",
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
            Text =
                "Korte activiteiten tijdens de middagpauze kunnen studenten makkelijker samenbrengen zonder grote tijdsinvestering.",
            Topic = apTopic23,
            ModerationStatus = ModerationStatus.Accepted
        };

        var apIdea22 = new Idea
        {
            Title = "Een gezellige ontmoetingsruimte",
            Text =
                "Een vaste plek met zitruimte en rustige sfeer zou studenten helpen om elkaar spontaan te ontmoeten.",
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


        var ideaModerationPrompt = new AiPrompt
        {
            Key = "idea_moderation",
            Name = "Idea moderation",
            PromptText = """
                         Je bent een moderator voor een studentenplatform.

                         Controleer en herschrijf een idee.

                         Titel: {title}
                         Inhoud: {text}

                         Geef ALLEEN JSON terug:

                         {
                           "isToxic": true/false,
                           "needsMoreDetail": true/false,
                           "explanation": "korte uitleg",
                           "suggestedTitle": "verbeterde titel",
                           "suggestedText": "verbeterde tekst"
                         }

                         Regels:
                         - isToxic = true bij scheldwoorden of beledigingen
                         - needsMoreDetail = true als het idee te kort of vaag is

                         - Als isToxic = true:
                           → herschrijf titel en tekst op een respectvolle manier
                           → voorbeeld:
                             "ik vind jou een klootzak" → "ik ben het niet eens met deze persoon"

                         - Als needsMoreDetail = true:
                           → maak het idee concreter

                         - Behoud altijd de betekenis
                         - Geen lege suggestedTitle of suggestedText
                         - Antwoord in het Nederlands
                         """,
            IsActive = true
        };

        var reactionModerationPrompt = new AiPrompt
        {
            Key = "reaction_moderation",
            Name = "Reaction moderation",
            PromptText = """
                         Je bent een moderator voor een studentenplatform.

                         Controleer en herschrijf de volgende tekst zodat deze respectvol is.

                         Originele tekst:
                         {text}

                         Geef ALLEEN JSON terug:

                         {
                           "isToxic": true/false,
                           "needsMoreDetail": false,
                           "explanation": "korte uitleg",
                           "suggestedText": "minder agressieve versie"
                         }

                         Regels:
                         - isToxic = true als er scheldwoorden of beledigingen in staan
                         - needsMoreDetail = altijd false

                         - Als isToxic = true:
                           → herschrijf de tekst op een respectvolle manier
                           → voorbeeld:
                             "je bent een klootzak" → "ik vind dit niet oké"

                         - Als isToxic = false:
                           → suggestedText = ""

                         - Behoud de betekenis, maar maak het beleefd
                         - Gebruik geen scheldwoorden in de herschreven versie
                         - Maak de tekst geschikt voor een schoolomgeving
                         - Antwoord in het Nederlands

                         BELANGRIJK:
                         - Geef enkel JSON terug
                         - Geen extra tekst buiten JSON
                         """,
            IsActive = true
        };

        var projectImagePrompt = new AiPrompt
        {
            Key = "project_image_generation",
            Name = "Project image generation",
            PromptText = """
                         Maak een visueel aantrekkelijke, moderne afbeelding voor een jongerenproject.

                         Projectnaam:
                         {projectName}

                         Regels:
                         - De afbeelding moet passen bij een digitaal participatieplatform voor jongeren.
                         - Gebruik een frisse, moderne en toegankelijke stijl.
                         - Toon geen tekst in de afbeelding.
                         - De afbeelding moet bruikbaar zijn als project cover.
                         """,
            IsActive = true
        };

        var projectIntroPrompt = new AiPrompt
        {
            Key = "project_intro_generation",
            Name = "Project intro generation",
            PromptText = """
                         Schrijf een korte introductietekst voor een jongerenparticipatieproject.

                         Projectnaam:
                         {projectName}

                         Regels:
                         - Schrijf in het Nederlands.
                         - Gebruik eenvoudige, motiverende taal voor jongeren/studenten.
                         - Leg kort uit waarom deelnemen belangrijk is.
                         - Maximum 5 zinnen.
                         - Geef alleen de introductietekst terug, geen extra uitleg.
                         """,
            IsActive = true
        };

        var surveyGeneraration = new AiPrompt
        {
            Key = "survey_generation",
            PromptText = """ 
                         Je bent een AI-assistent die helpt om vragenlijsten te maken voor participatieprojecten.

                         Maak op basis van de beschrijving een gestructureerde survey.

                         Antwoord ALLEEN met geldig JSON.
                         Gebruik GEEN markdown.
                         Gebruik GEEN ```json blokken.
                         Gebruik GEEN extra uitleg.

                         JSON schema:
                         {
                           "sections": [
                             {
                               "title": "string",
                               "questions": [
                                 {
                                   "title": "string",
                                   "type": "single | multiple | range | open",
                                   "answers": ["string"],
                                   "min": "string",
                                   "max": "string",
                                   "conditionals": []
                                 }
                               ]
                             }
                           ]
                         }

                         Regels:
                         - Maak exact {{questionAmount}} vragen in totaal.
                         - Verdeel deze vragen logisch over 1 tot 4 sections.
                         - Als er geen aantal vragen is meegegeven, geef dan standaard 10 vragen.
                         - Maak maximaal 20 vragen totaal.
                         - Gebruik duidelijke, neutrale en korte vragen.
                         - Gebruik type "single" voor één keuze.
                         - Gebruik type "multiple" voor meerdere keuzes.
                         - Gebruik type "range" voor schaalvragen.
                         - Gebruik type "open" voor open vragen.
                         - Bij "single" en "multiple" moeten answers ingevuld zijn.
                         - Bij "range" moeten min en max ingevuld zijn, bijvoorbeeld "1" en "5".
                         - Bij "open" moeten answers leeg zijn.
                         - Conditionals mogen leeg zijn.
                         - Gebruik Nederlands.
                         - Maak GEEN conditionele vragen.
                         - Zet bij elke question altijd exact dit veld: "conditionals": []
                         - Vul nooit "trigger", "ai" of "question" in.
                         - Conditionele vragen worden later manueel door de gebruiker toegevoegd.
                         - De vragenlijst moet passen bij deze beschrijving en question amount:

                         {{questionAmount}}
                         {{description}}
                         """
        };
        var ideaImprovementPrompt = new AiPrompt
        {
            Key = "idea_improvement",
            Name = "Idea improvement",
            PromptText = """
                         Je bent een AI-assistent voor een jongerenparticipatieplatform.

                         Herschrijf de titel en inhoud van het idee duidelijker en concreter.

                         Originele titel:
                         {title}

                         Originele inhoud:
                         {text}

                         Regels:
                         - Behoud de originele betekenis.
                         - Voeg geen volledig nieuwe feiten toe.
                         - Schrijf in het Nederlands.
                         - Geef ALLEEN geldige JSON terug.
                         - Geen markdown.
                         - Geen uitleg.

                         JSON schema:
                         {
                           "title": "verbeterde titel",
                           "text": "verbeterde inhoud"
                         }
                         """,
            IsActive = true
        };
        
        var projectTrendSummaryPrompt = new AiPrompt
        {
            Key = "project_trend_summary",
            Name = "Project trend summary",
            PromptText = """
                         Je bent een AI-analist voor een participatieplatform.

                         Analyseer de data van één project. Je krijgt:
                         - topics
                         - ideeën
                         - reacties
                         - ingevulde enquêtes met vragen en antwoorden

                         Doel:
                         De subadmin moet snel en overzichtelijk begrijpen:
                         - welke trends terugkomen
                         - hoe deelnemers denken over het project
                         - welke bezorgdheden vaak terugkomen
                         - welke ideeën steun krijgen
                         - wat uit de enquêtes blijkt
                         - welke acties de organisatie best kan nemen

                         Geef een duidelijke analyse in het Nederlands.

                         Structuur:
                         1. Algemene indruk
                         2. Belangrijkste trends
                         3. Wat blijkt uit de ideeën
                         4. Wat blijkt uit de reacties
                         5. Wat blijkt uit de enquêtes
                         6. Sentiment van de deelnemers
                         7. Veel voorkomende bezorgdheden
                         8. Populaire of sterke ideeën
                         9. Aanbevelingen voor de subadmin

                         Regels:
                         - Gebruik alleen de meegegeven data.
                         - Verzin geen cijfers.
                         - Als er weinig data is, zeg dat duidelijk.
                         - Maak het overzichtelijk met korte titels.
                         - Schrijf professioneel maar begrijpelijk.
                         - Geef geen JSON terug.
                         - Geef geen markdown codeblok terug.
                         - Geef concrete aanbevelingen, maar overdrijf niet.

                         DATA:
                         {{projectData}}
                         """,
            IsActive = true
        };
        
        var openQuestionSummaryPrompt = new AiPrompt
        {
            Key = "open_question_summary",
            PromptText = """
                         Vat de open antwoorden kort samen voor een subadmin.

                         Regels:
                         - Schrijf maximaal 5 korte bullets.
                         - Gebruik geen Markdown-opmaak zoals **vetgedrukt**.
                         - Gebruik geen lange alinea's.
                         - Maak geen personen herkenbaar.
                         - Schrijf concreet en duidelijk Nederlands.

                         Geef exact deze structuur:

                         Belangrijkste inzichten:
                         - ...
                         - ...
                         - ...

                         Korte conclusie:
                         ...

                         Vraag:
                         {{question}}

                         Antwoorden:
                         {{answers}}
                         """,
            IsActive = true
        };
        
        var surveyUsers = Enumerable.Range(1, 10).Select(i => new User
    {
        CookieIdentifier = $"seed-survey-user-{i}"
    })
    .ToList();

var surveyResponses = new List<SurveyResponse>
{
    new()
    {
        Project = project,
        User = surveyUsers[0],
        SubmittedAt = DateTime.UtcNow.AddDays(-10),
        Answers = new List<Answer>
        {
            new() { Question = mentalWellbeingQuestion, Text = "Goed" },
            new() { Question = stressSourcesQuestion, Text = "Studies / examens;Toekomstzorgen" },
            new() { Question = studyStressQuestion, Text = "4" },
            new() { Question = stressFollowUpQuestion, Text = "Een betere spreiding van deadlines zou helpen." },
            new() { Question = pressureQuestion, Text = "Soms wel, soms niet" },
            new() { Question = copingQuestion, Text = "Erover praten met vrienden/medestudenten;Sporten/bewegen" }
        }
    },
    new()
    {
        Project = project,
        User = surveyUsers[1],
        SubmittedAt = DateTime.UtcNow.AddDays(-9),
        Answers = new List<Answer>
        {
            new() { Question = mentalWellbeingQuestion, Text = "Zeer slecht" },
            new() { Question = stressSourcesQuestion, Text = "Financiële zorgen;Combinatie studie-werk" },
            new() { Question = studyStressQuestion, Text = "5" },
            new() { Question = stressFollowUpQuestion, Text = "Meer begrip voor studenten die werken naast hun studies." },
            new() { Question = pressureQuestion, Text = "Eerder niet" },
            new() { Question = copingQuestion, Text = "Afleiding zoeken;Studietaken uitstellen/vermijden" }
        }
    },
    new()
    {
        Project = project,
        User = surveyUsers[2],
        SubmittedAt = DateTime.UtcNow.AddDays(-8),
        Answers = new List<Answer>
        {
            new() { Question = mentalWellbeingQuestion, Text = "Neutraal" },
            new() { Question = stressSourcesQuestion, Text = "Sociale druk / eenzaamheid / relaties" },
            new() { Question = studyStressQuestion, Text = "3" },
            new() { Question = pressureQuestion, Text = "Ja, meestal wel" },
            new() { Question = copingQuestion, Text = "Erover praten met vrienden/medestudenten;Bewust rust inplannen" }
        }
    },
    new()
    {
        Project = project,
        User = surveyUsers[3],
        SubmittedAt = DateTime.UtcNow.AddDays(-7),
        Answers = new List<Answer>
        {
            new() { Question = mentalWellbeingQuestion, Text = "Eerder slecht" },
            new() { Question = stressSourcesQuestion, Text = "Studies / examens;Fysieke of mentale gezondheid" },
            new() { Question = studyStressQuestion, Text = "4" },
            new() { Question = stressFollowUpQuestion, Text = "Duidelijkere planning en minder taken tegelijk." },
            new() { Question = pressureQuestion, Text = "Soms wel, soms niet" },
            new() { Question = copingQuestion, Text = "Professionele hulp zoeken;Bewust rust inplannen" }
        }
    },
    new()
    {
        Project = project,
        User = surveyUsers[4],
        SubmittedAt = DateTime.UtcNow.AddDays(-6),
        Answers = new List<Answer>
        {
            new() { Question = mentalWellbeingQuestion, Text = "Goed" },
            new() { Question = stressSourcesQuestion, Text = "Toekomstzorgen" },
            new() { Question = studyStressQuestion, Text = "3" },
            new() { Question = pressureQuestion, Text = "Ja, meestal wel" },
            new() { Question = copingQuestion, Text = "Sporten/bewegen;Bewust rust inplannen" }
        }
    },
    new()
    {
        Project = project,
        User = surveyUsers[5],
        SubmittedAt = DateTime.UtcNow.AddDays(-5),
        Answers = new List<Answer>
        {
            new() { Question = mentalWellbeingQuestion, Text = "Zeer goed" },
            new() { Question = stressSourcesQuestion, Text = "Studies / examens" },
            new() { Question = studyStressQuestion, Text = "2" },
            new() { Question = pressureQuestion, Text = "Ja, meestal wel" },
            new() { Question = copingQuestion, Text = "Erover praten met ouders/familie/partner;Sporten/bewegen" }
        }
    },
    new()
    {
        Project = project,
        User = surveyUsers[6],
        SubmittedAt = DateTime.UtcNow.AddDays(-4),
        Answers = new List<Answer>
        {
            new() { Question = mentalWellbeingQuestion, Text = "Eerder slecht" },
            new() { Question = stressSourcesQuestion, Text = "Thuissituatie;Financiële zorgen" },
            new() { Question = studyStressQuestion, Text = "5" },
            new() { Question = stressFollowUpQuestion, Text = "Meer flexibele deadlines bij persoonlijke problemen." },
            new() { Question = pressureQuestion, Text = "Helemaal niet" },
            new() { Question = copingQuestion, Text = "Ik weet niet goed wat ik kan of moet doen;Afleiding zoeken" }
        }
    },
    new()
    {
        Project = project,
        User = surveyUsers[7],
        SubmittedAt = DateTime.UtcNow.AddDays(-3),
        Answers = new List<Answer>
        {
            new() { Question = mentalWellbeingQuestion, Text = "Goed" },
            new() { Question = stressSourcesQuestion, Text = "Combinatie studie-werk" },
            new() { Question = studyStressQuestion, Text = "3" },
            new() { Question = pressureQuestion, Text = "Soms wel, soms niet" },
            new() { Question = copingQuestion, Text = "Erover praten met vrienden/medestudenten;Afleiding zoeken" }
        }
    },
    new()
    {
        Project = project,
        User = surveyUsers[8],
        SubmittedAt = DateTime.UtcNow.AddDays(-2),
        Answers = new List<Answer>
        {
            new() { Question = mentalWellbeingQuestion, Text = "Neutraal" },
            new() { Question = stressSourcesQuestion, Text = "Sociale druk / eenzaamheid / relaties;Toekomstzorgen" },
            new() { Question = studyStressQuestion, Text = "3" },
            new() { Question = pressureQuestion, Text = "Soms wel, soms niet" },
            new() { Question = copingQuestion, Text = "Afleiding zoeken;Bewust rust inplannen" }
        }
    },
    new()
    {
        Project = project,
        User = surveyUsers[9],
        SubmittedAt = DateTime.UtcNow.AddDays(-1),
        Answers = new List<Answer>
        {
            new() { Question = mentalWellbeingQuestion, Text = "Zeer slecht" },
            new() { Question = stressSourcesQuestion, Text = "Fysieke of mentale gezondheid;Studies / examens" },
            new() { Question = studyStressQuestion, Text = "5" },
            new() { Question = stressFollowUpQuestion, Text = "Sneller toegang tot begeleiding en minder druk in examenperiodes." },
            new() { Question = pressureQuestion, Text = "Eerder niet" },
            new() { Question = copingQuestion, Text = "Professionele hulp zoeken;Afleiding zoeken" }
        }
    }
};


        /* =========================
           DATABASE INSERT
        ========================= */


        dbContext.AiPrompts.Add(ideaModerationPrompt);
        dbContext.AiPrompts.Add(reactionModerationPrompt);
        dbContext.AiPrompts.Add(projectImagePrompt);
        dbContext.AiPrompts.Add(projectIntroPrompt);
        dbContext.AiPrompts.Add(surveyGeneraration);
        dbContext.AiPrompts.Add(ideaImprovementPrompt);
        dbContext.AiPrompts.Add(projectTrendSummaryPrompt);
        dbContext.AiPrompts.Add(openQuestionSummaryPrompt);
        dbContext.Platforms.Add(platform);
        dbContext.SubPlatforms.Add(subPlatform);
        dbContext.SubPlatforms.Add(apSubPlatform);
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
        dbContext.GeneralAdmins.Add(generalAdmin);
        dbContext.SubAdmins.Add(kdgAdmin);
        dbContext.SubAdmins.Add(apAdmin);



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

        dbContext.Users.AddRange(surveyUsers);
        dbContext.SurveyResponses.AddRange(surveyResponses);
        
        dbContext.SaveChanges();
    }
}