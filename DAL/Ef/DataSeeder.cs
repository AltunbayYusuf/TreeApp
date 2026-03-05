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
                Description =
                    "Hoe zou jij je mentaal welzijn in de voorbije maand omschrijven?\n" +
                    "- Zeer goed\n- Goed\n- Neutraal\n- Eerder slecht\n- Zeer slecht",
                QuestionType = QuestionType.Range,
                Section = section
            });

            questions.Add(new Question
            {
                Description =
                    "Wat zijn voor jou momenteel de grootste bronnen van stress? (meerdere mogelijk)\n" +
                    "- Studies / examens\n- Combinatie studie-werk\n- Financiële zorgen\n- Sociale druk / eenzaamheid / relaties\n" +
                    "- Fysieke of mentale gezondheid\n- Thuissituatie\n- Toekomstzorgen\n- Andere: ____",
                QuestionType = QuestionType.MultipleChoice,
                Section = section
            });

            questions.Add(new Question
            {
                Description =
                    "In welke mate ervaar jij stress door je studies? (1 = geen stress, 10 = extreem veel stress)",
                QuestionType = QuestionType.Range,
                Section = section
            });

            questions.Add(new Question
            {
                Description =
                    "Heb je het gevoel dat je de mentale druk die je ervaart meestal aankan?\n" +
                    "- Ja, meestal wel\n- Soms wel, soms niet\n- Eerder niet\n- Helemaal niet",
                QuestionType = QuestionType.SingleChoice,
                Section = section
            });

            questions.Add(new Question
            {
                Description =
                    "Wat doe jij meestal wanneer je het mentaal moeilijk hebt? (meerdere mogelijk)\n" +
                    "- Ik weet niet goed wat ik kan of moet doen\n- Erover praten met ouders/familie/partner\n" +
                    "- Erover praten met vrienden/medestudenten\n- Professionele hulp zoeken\n- Sporten/bewegen\n" +
                    "- Afleiding zoeken\n- Studietaken uitstellen/vermijden\n- Bewust rust inplannen\n" +
                    "- Medicatie/alcohol/andere middelen\n- Iets anders: ____",
                QuestionType = QuestionType.MultipleChoice,
                Section = section
            });

            questionList.Sections = new List<Section> { section };

            project.QuestionList = questionList;

            
            //dit is om tussen topics kiezen. voorlopig nogniet nodif
            project.Topics = new List<Topic>
            {
                new Topic { Theme = "Stress & studies", Project = project },
                new Topic { Theme = "Ondersteuning hogeschool", Project = project },
                new Topic { Theme = "Ideeën voor acties (ideation)", Project = project }
            };

            dbContext.Platforms.Add(platform);
            dbContext.SubPlatforms.Add(subPlatform);
            dbContext.Projects.Add(project);

            
            dbContext.QuestionList.Add(questionList);
            dbContext.Section.Add(section);
            dbContext.Questions.AddRange(questions);

            dbContext.SaveChanges();
        }
    }