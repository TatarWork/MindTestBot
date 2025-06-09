using MindBot.Services.Models;

namespace MindBot.Services.Interfaces
{
    public interface IQuestionService
    {
        /// <summary>
        /// Получение списка вопросов для тестирования
        /// </summary>
        /// <returns></returns>
        List<QuestionModel> GetQuestions();
    }
}
