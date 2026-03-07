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
                Language = Language.Nl,
                Platform = platform
            };

            var project = new Project
            {
                Introduction =
                    "Jouw welzijn telt. Denk met ons mee.\n\n" +
                    "Deze vragenlijst is anoniem en helpt ons een actieplan mentaal welzijn te maken.",
                Status = Status.Active,
                Type = ProjectType.VerticalScroll,
                Prompt = "",                 // AI aan de kant gezet
                Duration = 10,               // minuten (demo)
                ReleaseDate = DateTime.UtcNow,
                SubPlatform = subPlatform
            };

          
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
                Section = section
            });

            questions.Add(new Question
            {
                Description = "Wat zijn voor jou momenteel de grootste bronnen van stress?",
                QuestionType = QuestionType.MultipleChoice,
                Section = section
            });

            questions.Add(new Question
            {
                Description = "In welke mate ervaar jij stress door je studies?",
                QuestionType = QuestionType.Range,
                Section = section
            });

            questions.Add(new Question
            {
                Description = "Heb je het gevoel dat je de mentale druk die je ervaart meestal aankan?",
                QuestionType = QuestionType.SingleChoice,
                Section = section
            });

            questions.Add(new Question
            {
                Description = "Wat doe jij meestal wanneer je het mentaal moeilijk hebt?",
                QuestionType = QuestionType.OpenQuestion,
                Section = section
            }); 
            
           

            questionList.Sections = new List<Section> { section };

            project.QuestionList = questionList;

            
            //dit is om tussen topics kiezen. voorlopig nogniet nodif
            var topicStress = new Topic
            {
                Theme = "Stress & studies",
                Project = project
            };

            var topicSupport = new Topic
            {
                Theme = "Ondersteuning hogeschool",
                Project = project
            };

            var topicActions = new Topic
            {
                Theme = "Ideeën voor acties (ideation)",
                Project = project
            };

            var topicDrugs = new Topic
            {
                Theme = "Het gebruik van drugs op school",
                Project = project
            };

            project.Topics = new List<Topic>
            {
                topicStress,
                topicSupport,
                topicActions,
                topicDrugs
            };
            
            var idea1 = new Idea
            {
                Title = "Spreid deadlines beter",
                Text = "Veel stress komt doordat meerdere deadlines in dezelfde week vallen.",
                Topic = topicStress,
                ModerationStatus = default
            };

            var idea2 = new Idea
            {
                Title = "Extra stille blokruimtes",
                Text = "Tijdens examens zijn er te weinig rustige studieplaatsen.",
                Topic = topicStress,
                ModerationStatus = default
            };

            var idea3 = new Idea
            {
                Title = "Meer zichtbaarheid van hulpdiensten",
                Text = "Veel studenten weten niet waar ze terecht kunnen bij mentale problemen.",
                Topic = topicSupport,
                ModerationStatus = default
            };

            var idea4 = new Idea
            {
                Title = "Mentale welzijnsweek",
                Text = "Een week met workshops rond stress, slaap en planning.",
                Topic = topicActions,
                ModerationStatus = default
            };

            var idea5 = new Idea
            {
                Title = "Betere preventie rond middelengebruik",
                Text = "Meer informatie en begeleiding rond alcohol en drugs.",
                Topic = topicDrugs,
                ModerationStatus = default
            };

        var reaction1 = new Reaction
        {
            Text = "Dit zou echt helpen, zeker in de examenperiode wanneer alles samenkomt.",
            Idea = idea1,
            ModerationStatus = default
        };

        var reaction2 = new Reaction
        {
            Text = "Mee eens. Vooral groepswerken en individuele deadlines zitten nu vaak te dicht op elkaar.",
            Idea = idea1,
            ModerationStatus = default
        };

        var reaction3 = new Reaction
        {
            Text = "Goede suggestie. Ik vind ook dat stille ruimtes vaak te snel volzet zijn.",
            Idea = idea2,
            ModerationStatus = default
        };

        var reaction4 = new Reaction
        {
            Text = "Ik wist eerlijk gezegd ook niet goed waar ik terechtkon, dus een centrale pagina lijkt me nuttig.",
            Idea = idea3,
            ModerationStatus = default
        };
        

        idea1.Reactions = new List<Reaction> { reaction1, reaction2 };
        idea2.Reactions = new List<Reaction> { reaction3 };
        idea3.Reactions = new List<Reaction> { reaction4 };
        idea4.Reactions = new List<Reaction>();
        idea5.Reactions = new List<Reaction>();
        

        var ideas = new List<Idea>
        {
            idea1, idea2, idea3, idea4, idea5
        };

        var reactions = new List<Reaction>
        {
            reaction1, reaction2, reaction3, reaction4
        };

            dbContext.Platforms.Add(platform);
            dbContext.SubPlatforms.Add(subPlatform);
            dbContext.Projects.Add(project);

            
            dbContext.QuestionList.Add(questionList);
            dbContext.Section.Add(section);
            dbContext.Questions.AddRange(questions);

            dbContext.Topics.AddRange(project.Topics);
            dbContext.Ideas.AddRange(ideas);
            dbContext.Reactions.AddRange(reactions);
            
            dbContext.SaveChanges();
        }
    }