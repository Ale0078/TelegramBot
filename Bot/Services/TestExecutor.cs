using AutoMapper;
using Microsoft.EntityFrameworkCore;

using Bot.Models;

namespace Bot.Services
{
    public class TestExecutor
    {
        private readonly IMapper _mapper;

        private List<Question> Test { get; set; }

        public TestExecutor(Bot.Entities.ApplicationContext context, IMapper mapper)
        {
            _mapper = mapper;

            Test = _mapper.Map<List<Question>>(context.Questions
                .Include(question => question.Answers)
                .Include(question => question.SuccessMessage)
                .Include(question => question.FailMessage)
                .AsNoTracking());
        }
    }
}
