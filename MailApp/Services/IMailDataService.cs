using MailApp.Filters;
using MailApp.Models;

namespace MailApp.Services;

public interface IMailDataService
{
    public (IEnumerable<Letter> letters, long fullCount) GetLetters(PaginationFilter pagination, string? sender,
        string? recipient, string? tag, DateFilter dateRange);
    
    public IndexLetterDto GetLetterById(int id);
    
    public int CreateLetter(CreateLetterDto letter);
    
    public IndexLetterDto UpdateLetter(int id, UpdateLetterDto updateLetterParams);
    
    public void RemoveLetter(int id);
}
