namespace Bot.Entities
{
    public class DatabaseInitializer
    {
        public DatabaseInitializer(ApplicationContext context)
        {
            InitialaziDatabase(context);
        }

        private void InitialaziDatabase(ApplicationContext context) 
        {
            SuccessMessage successMessage1 = new() { Id = new Guid("00000000-0000-0000-0000-000000000001"), Message = "Супер! Твой ответ правильный✅" };
            SuccessMessage successMessage2 = new() { Id = new Guid("00000000-0000-0000-0000-000000000002"), Message = "Молодец, твой ответ верный! Продолжай в том же духе🙌" };
            SuccessMessage successMessage3 = new() { Id = new Guid("00000000-0000-0000-0000-000000000003"), Message = "Да, ты прав!" };
            SuccessMessage successMessage4 = new() { Id = new Guid("00000000-0000-0000-0000-000000000004"), Message = "Отлично! Этот ответ верный✅" };
            SuccessMessage successMessage5 = new() { Id = new Guid("00000000-0000-0000-0000-000000000005"), Message = "Верно! Ты молодец💪 Теперь предложение на перевод👇" };
            SuccessMessage successMessage6 = new() { Id = new Guid("00000000-0000-0000-0000-000000000006"), Message = "Это был сложный вопрос, но ты справился с ним✅" };
            SuccessMessage successMessage7 = new() { Id = new Guid("00000000-0000-0000-0000-000000000007"), Message = "Правильно! Но следующее задание будет посложнее🤔" };
            SuccessMessage successMessage8 = new() { Id = new Guid("00000000-0000-0000-0000-000000000008"), Message = "Да, эта тема сложная, но ты ответил правильно✅" };
            SuccessMessage successMessage9 = new() { Id = new Guid("00000000-0000-0000-0000-000000000009"), Message = "Ехууу⚡⚡Твой ответ верный, остался последний вопрос и ты узнаешь свой результат💪" };
            SuccessMessage successMessage10 = new() { Id = new Guid("00000000-0000-0000-0000-000000000010"), Message = "Yes, на последний вопрос ты ответил верно✔🔥" };

            FailMessage failMessage1 = new() { Id = new Guid("00000000-0000-0000-0000-000000000001"), Message = "Ты не совсем внимательный🤔\n\nЗдесь тебе нужно обратить на разницу между past simple и  present perfect🥰" };
            FailMessage failMessage2 = new() { Id = new Guid("00000000-0000-0000-0000-000000000002"), Message = "Не совсем так, тебе нужно повторить конструкцию Present Perfect Continuous ☝️" };
            FailMessage failMessage3 = new() { Id = new Guid("00000000-0000-0000-0000-000000000003"), Message = "К сожалению нет😔" };
            FailMessage failMessage4 = new() { Id = new Guid("00000000-0000-0000-0000-000000000004"), Message = "Нет, здесь тебе нужно повторить неправильные глаголы😉" };
            FailMessage failMessage5 = new() { Id = new Guid("00000000-0000-0000-0000-000000000005"), Message = "Нет, этот ответ неверный😬\n\nВспомни  использование времени  Present Perfect Continuous " };
            FailMessage failMessage6 = new() { Id = new Guid("00000000-0000-0000-0000-000000000006"), Message = "Этот ответ неверный, но ты не расстраивайся это  был сложный вопрос⚡️\n\nНо повтори тему Indirect Speech ✔️" };
            FailMessage failMessage7 = new() { Id = new Guid("00000000-0000-0000-0000-000000000007"), Message = "Не совсем так, соберись! Потому что следующее задание будет посложнее🤔 и не забудь повторить тему Passive Voice ❗️" };
            FailMessage failMessage8 = new() { Id = new Guid("00000000-0000-0000-0000-000000000008"), Message = "Эта тема действительно сложная и возможно ты не знаешь ее, твой не верный 😬 Повтори или изучи тему Conditionals , она пригодится тебе😉" };
            FailMessage failMessage9 = new() { Id = new Guid("00000000-0000-0000-0000-000000000009"), Message = "К сожалению нет, твой ответ неверный, тебе нужно вспомнить  использование времени Future perfect, но остался последний вопрос и ты узнаешь свой результат💪" };
            FailMessage failMessage10 = new() { Id = new Guid("00000000-0000-0000-0000-000000000010"), Message = "No, на последний вопрос ты ответил неверно☝️Повтори неправильные глаголы😉" };

            Question question1 = new() { Id = new Guid("00000000-0000-0000-0000-000000000001"), Content = "Переведи это предложение👇\n\nОн приехал", Mark = 1, FailMessageId = failMessage1.Id, SuccessMessageId = successMessage1.Id };
            Question question2 = new() { Id = new Guid("00000000-0000-0000-0000-000000000002"), Content = "Но давай дальше, вставь пропущенное слово👇\n\nHe has been ______ a letter for 20 minutes", Mark = 1, FailMessageId = failMessage2.Id, SuccessMessageId = successMessage2.Id };
            Question question3 = new() { Id = new Guid("00000000-0000-0000-0000-000000000003"), Content = "Вставь пропущенное слово\n\nShe will ____ soon", Mark = 1, FailMessageId = failMessage3.Id, SuccessMessageId = successMessage3.Id };
            Question question4 = new() { Id = new Guid("00000000-0000-0000-0000-000000000004"), Content = "Повтори реконструкцию Future Simple ✔️\n\nТеперь выбери третью форму неправильного глагола WRITE", Mark = 1, FailMessageId = failMessage4.Id, SuccessMessageId = successMessage4.Id };
            Question question5 = new() { Id = new Guid("00000000-0000-0000-0000-000000000005"), Content = "В каком времени написано это предложение?\n\nОна работает в этой компании 2 года 👇", Mark = 1, FailMessageId = failMessage5.Id, SuccessMessageId = successMessage5.Id };
            Question question6 = new() { Id = new Guid("00000000-0000-0000-0000-000000000006"), Content = "Теперь предложение на перевод👇\n\nОн сказал, что перезвонит.", Mark = 1, FailMessageId = failMessage6.Id, SuccessMessageId = successMessage6.Id };
            Question question7 = new() { Id = new Guid("00000000-0000-0000-0000-000000000007"), Content = "Переведи это предложение👇\n\nНас пригласили туда", Mark = 1, FailMessageId = failMessage7.Id, SuccessMessageId = successMessage7.Id };
            Question question8 = new() { Id = new Guid("00000000-0000-0000-0000-000000000008"), Content = "Выбери какой conditional здесь правильно употребить 👇\n\nЕсли бы он был богат, он бы выкупил вчера эту тачку", Mark = 1, FailMessageId = failMessage8.Id, SuccessMessageId = successMessage8.Id };
            Question question9 = new() { Id = new Guid("00000000-0000-0000-0000-000000000009"), Content = "Теперь ответь в каком времени написано это предложение?\n\nК вечеру мы встретимся", Mark = 1, FailMessageId = failMessage9.Id, SuccessMessageId = successMessage9.Id };
            Question question10 = new() { Id = new Guid("00000000-0000-0000-0000-000000000010"), Content = "Выбери третью форму неправильного глагола FLY👇", Mark = 1, FailMessageId = failMessage10.Id, SuccessMessageId = successMessage10.Id };

            Answer answer1_1 = new() { Id = new Guid("00000000-0000-0000-0001-000000000001"), Content = "He came", IsCorrect = false, QuestionId = question1.Id };
            Answer answer1_2 = new() { Id = new Guid("00000000-0000-0000-0001-000000000002"), Content = "He already came", IsCorrect = false, QuestionId = question1.Id };
            Answer answer1_3 = new() { Id = new Guid("00000000-0000-0000-0001-000000000003"), Content = "He has come", IsCorrect = true, QuestionId = question1.Id };

            Answer answer2_1 = new() { Id = new Guid("00000000-0000-0000-0002-000000000001"), Content = "writing", IsCorrect = true, QuestionId = question2.Id };
            Answer answer2_2 = new() { Id = new Guid("00000000-0000-0000-0002-000000000002"), Content = "written", IsCorrect = false, QuestionId = question2.Id };
            Answer answer2_3 = new() { Id = new Guid("00000000-0000-0000-0002-000000000003"), Content = "wrote", IsCorrect = false, QuestionId = question2.Id };

            Answer answer3_1 = new() { Id = new Guid("00000000-0000-0000-0003-000000000001"), Content = "return", IsCorrect = true, QuestionId = question3.Id };
            Answer answer3_2 = new() { Id = new Guid("00000000-0000-0000-0003-000000000002"), Content = "be returning", IsCorrect = false, QuestionId = question3.Id };
            Answer answer3_3 = new() { Id = new Guid("00000000-0000-0000-0003-000000000003"), Content = "have returned", IsCorrect = false, QuestionId = question3.Id };

            Answer answer4_1 = new() { Id = new Guid("00000000-0000-0000-0004-000000000001"), Content = "write", IsCorrect = false, QuestionId = question4.Id };
            Answer answer4_2 = new() { Id = new Guid("00000000-0000-0000-0004-000000000002"), Content = "wrote", IsCorrect = false, QuestionId = question4.Id };
            Answer answer4_3 = new() { Id = new Guid("00000000-0000-0000-0004-000000000003"), Content = "written", IsCorrect = true, QuestionId = question4.Id };

            Answer answer5_1 = new() { Id = new Guid("00000000-0000-0000-0005-000000000001"), Content = "Prsent Simple", IsCorrect = false, QuestionId = question5.Id };
            Answer answer5_2 = new() { Id = new Guid("00000000-0000-0000-0005-000000000002"), Content = "Prsent Continuous", IsCorrect = false, QuestionId = question5.Id };
            Answer answer5_3 = new() { Id = new Guid("00000000-0000-0000-0005-000000000003"), Content = "Prsent Perfect Continuous", IsCorrect = true, QuestionId = question5.Id };

            Answer answer6_1 = new() { Id = new Guid("00000000-0000-0000-0006-000000000001"), Content = "He said that he will call back", IsCorrect = false, QuestionId = question6.Id };
            Answer answer6_2 = new() { Id = new Guid("00000000-0000-0000-0006-000000000002"), Content = "He said that he can call back", IsCorrect = false, QuestionId = question6.Id };
            Answer answer6_3 = new() { Id = new Guid("00000000-0000-0000-0006-000000000003"), Content = "He said that he would call back", IsCorrect = true, QuestionId = question6.Id };

            Answer answer7_1 = new() { Id = new Guid("00000000-0000-0000-0007-000000000001"), Content = "We invited there", IsCorrect = false, QuestionId = question7.Id };
            Answer answer7_2 = new() { Id = new Guid("00000000-0000-0000-0007-000000000002"), Content = "We were invited there", IsCorrect = true, QuestionId = question7.Id };
            Answer answer7_3 = new() { Id = new Guid("00000000-0000-0000-0007-000000000003"), Content = "We had been invited there", IsCorrect = false, QuestionId = question7.Id };

            Answer answer8_1 = new() { Id = new Guid("00000000-0000-0000-0008-000000000001"), Content = "2 conditional", IsCorrect = false, QuestionId = question8.Id };
            Answer answer8_2 = new() { Id = new Guid("00000000-0000-0000-0008-000000000002"), Content = "3 conditional", IsCorrect = false, QuestionId = question8.Id };
            Answer answer8_3 = new() { Id = new Guid("00000000-0000-0000-0008-000000000003"), Content = "Mixed conditional", IsCorrect = true, QuestionId = question8.Id };

            Answer answer9_1 = new() { Id = new Guid("00000000-0000-0000-0009-000000000001"), Content = "Future Simple", IsCorrect = false, QuestionId = question9.Id };
            Answer answer9_2 = new() { Id = new Guid("00000000-0000-0000-0009-000000000002"), Content = "Future Perfect", IsCorrect = true, QuestionId = question9.Id };
            Answer answer9_3 = new() { Id = new Guid("00000000-0000-0000-0009-000000000003"), Content = "Future Perfect Continuous", IsCorrect = false, QuestionId = question9.Id };

            Answer answer10_1 = new() { Id = new Guid("00000000-0000-0000-0010-000000000001"), Content = "Flew", IsCorrect = false, QuestionId = question10.Id };
            Answer answer10_2 = new() { Id = new Guid("00000000-0000-0000-0010-000000000002"), Content = "Filed", IsCorrect = false, QuestionId = question10.Id };
            Answer answer10_3 = new() { Id = new Guid("00000000-0000-0000-0010-000000000003"), Content = "Flown", IsCorrect = true, QuestionId = question10.Id };

            context.SuccessMessages.AddRange(successMessage1, successMessage2, successMessage3, successMessage4, successMessage5, successMessage6, successMessage7, successMessage8, successMessage9, successMessage10);

            context.FailMessages.AddRange(failMessage1, failMessage2, failMessage3, failMessage4, failMessage5, failMessage6, failMessage7, failMessage8, failMessage9, failMessage10);

            context.Questions.AddRange(question1, question2, question3, question4, question5, question6, question7, question8, question9, question10);

            context.Answers.AddRange(answer1_1, answer1_2, answer1_3,
                answer2_1, answer2_2, answer2_3,
                answer3_1, answer3_2, answer3_3,
                answer4_1, answer4_2, answer4_3,
                answer5_1, answer5_2, answer5_3,
                answer6_1, answer6_2, answer6_3,
                answer7_1, answer7_2, answer7_3,
                answer8_1, answer8_2, answer8_3,
                answer9_1, answer9_2, answer9_3,
                answer10_1, answer10_2, answer10_3);

            context.SaveChanges();
        }
    }
}
