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

            _weightOfTest = 10;
        }

        public bool DoesUserFinishTest(long userId) 
        {
            return _participants[userId].State == _weightOfTest;
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
            MarkScore userScore = _participants[userId];
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

            if (Test.IndexOf(userScore.CurrentQuestion) + 1 < Test.Count)
            {
                userScore.CurrentQuestion = Test[Test.IndexOf(userScore.CurrentQuestion) + 1];
            }

            return checkResult;
        }

        public Question GetCurrentQuestion(long userId) 
        {
            return _participants[userId].CurrentQuestion;
        }

        public bool RemoveUser(long userId) 
        {
            return _participants.Remove(userId, out _);
        }

        public string FinishTest(long userId)
        {
            int score = _participants[userId].Score;

            RemoveUser(userId);

            return score switch 
            {
                0 => string.Format(_resourceReader["TestScoreZero"], score),
                1 => string.Format(_resourceReader["TestScoreOne"], score),
                2 => string.Format(_resourceReader["TestScoreTwo"], score),
                3 or 4 => string.Format(_resourceReader["TestScoreBetweenThreeFour"], score),
                5 or 6 => string.Format(_resourceReader["TestScoreBetweenFiveSix"], score),
                7 or 8 => string.Format(_resourceReader["TestScoreBetweenSevenEight"], score),
                9 or 10 => string.Format(_resourceReader["TestScoreBetweenNineTen"], score),
                _ => "Not found"
            };
        }
    }
}
