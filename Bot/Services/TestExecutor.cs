using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

using Bot.Models;
using Bot.Datas;

namespace Bot.Services
{
    public class TestExecutor
    {
        private readonly IMapper _mapper;
        private readonly ConcurrentDictionary<long, MarkScore> _participants;
        private readonly ResourceReader _resourceReader;

        private int _weightOfTest;

        private List<Question> Test { get; set; }

        public event Action FinishTestEvent;

        public TestExecutor(Bot.Entities.ApplicationContext context, IMapper mapper, ResourceReader resourceReader)
        {
            _mapper = mapper;
            _resourceReader = resourceReader;

            Test = _mapper.Map<List<Question>>(context.Questions
                .Include(question => question.Answers)
                .Include(question => question.SuccessMessage)
                .Include(question => question.FailMessage)
                .AsNoTracking());

            _participants = new ConcurrentDictionary<long, MarkScore>();
        }

        public bool HasUser(long id) 
        {
            return _participants.ContainsKey(id);
        }

        public bool AddUser(long id) 
        {
            MarkScore score = new MarkScore();

            score.CurrentQuestion = Test.First();

            return _participants.TryAdd(id, score);
        }

        public string CheckAnswer(long userId, string answer) 
        {
            MarkScore userScore = _participants.First(key => key.Key == userId).Value;
            Answer userAnswer = userScore.CurrentQuestion.Answers.First(answerModel => answerModel.Content == answer);

            string checkResult;

            if (userAnswer.IsCorrect)
            {
                userScore.Score++;

                checkResult = userScore.CurrentQuestion.SuccessMessage.Message;
            }
            else 
            {
                checkResult = userScore.CurrentQuestion.FailMessage.Message;
            }

            userScore.State++;

            if (userScore.State == _weightOfTest)
            {
                return FinishTest(userId, userScore);
            }

            userScore.CurrentQuestion = Test[Test.IndexOf(userScore.CurrentQuestion) + 1];

            return checkResult;
        }

        public Question GetCurrentQuestion(long userId) 
        {
            return _participants.First(key => key.Key == userId).Value.CurrentQuestion;
        }

        public bool RemoveUser(long userId) 
        {
            return _participants.Remove(userId, out _);
        }

        private string FinishTest(long userId, MarkScore score) 
        {
            try
            {
                return _resourceReader["FinishTest"];
            }
            finally
            {
                FinishTestEvent?.Invoke();

                RemoveUser(userId);
            }
        }
    }
}
