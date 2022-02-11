using MailApp.Data;
using MailApp.Filters;
using MailApp.Helpers;
using MailApp.Models;
using MailApp.Services;
using MailApp.Wrappers;
using Microsoft.AspNetCore.Mvc;

namespace MailApp.Controllers;

[ApiController, Route("letters")]
public class LetterController : ControllerBase
{
    private readonly ILogger<LetterController> _logger;
    private readonly IUriService _uriService;
    private readonly MailData _mailData;

    public LetterController(ILogger<LetterController> logger, IConfiguration configuration, IUriService uriService)
    {
        _logger = logger;
        _uriService = uriService;
        _mailData = new MailData(configuration.GetConnectionString("Postgresql"));
    }

    /// <summary>
    /// Get a list of letters which can be filtered via query parameters
    /// </summary>
    /// <response code="200">If letters found</response>
    [HttpGet]
    public IActionResult GetLetters([FromQuery] PaginationFilter pagination, [FromQuery] string? sender,
        [FromQuery] string? recipient, [FromQuery] string? tag, [FromQuery] DateFilter dateRange)
    {
        var (letters, fullCount) = _mailData.GetLetters(pagination, sender, recipient, tag, dateRange);
        var pagedResponse = PaginationHelper.CreatePagedResponse(letters, pagination, fullCount, _uriService,
            Request.Path.Value ?? string.Empty);
        return Ok(pagedResponse);
    }

    /// <summary>
    /// Get letter by id
    /// </summary>
    /// <response code="200">If letter found</response>
    /// <response code="404">If letter with specified Id doesnt exist</response>
    [HttpGet("{id:int}")]
    public IActionResult GetLetterById(int id)
    {
        IndexLetterDto indexLetter;
        try
        {
            indexLetter = _mailData.GetLetterById(id);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ApiResponse {Succeeded = false, Message = "Letter doesnt exist."});
        }

        return Ok(new ApiResponse(indexLetter));
    }

    /// <summary>
    /// Create letter
    /// </summary>
    /// <response code="201">If letter created</response>
    [HttpPost]
    public IActionResult CreateLetter(CreateLetterDto createLetter)
    {
        var letterId = _mailData.CreateLetter(createLetter);
        var createdLetter = _mailData.GetLetterById(letterId);
        return CreatedAtAction(nameof(GetLetterById), new {id = letterId}, new ApiResponse(createdLetter));
    }

    /// <summary>
    /// Partial letter update by Id
    /// </summary>
    /// <response code="200">If letter updated</response>
    /// <response code="404">If letter with specified Id doesnt exist</response>
    [HttpPatch("{id:int}")]
    public IActionResult UpdateLetter(int id, UpdateLetterDto updateLetter)
    {
        IndexLetterDto updatedIndexLetter;
        try
        {
            updatedIndexLetter = _mailData.UpdateLetter(id, updateLetter);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ApiResponse {Succeeded = false, Message = "Letter doesnt exist."});
        }

        return Ok(new ApiResponse(updatedIndexLetter) {Message = "Letter updated successfully."});
    }

    /// <summary>
    /// Remove letter which is searched by Id
    /// </summary>
    /// <response code="200">If letter removed successfully</response>
    /// <response code="404">If letter with specified Id doesnt exist</response>
    [HttpDelete("{id:int}")]
    public IActionResult RemoveLetter(int id)
    {
        try
        {
            _mailData.RemoveLetter(id);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ApiResponse {Succeeded = false, Message = "Letter doesnt exist."});
        }

        return Ok(new ApiResponse {Message = "Letter removed successfully."});
    }
}
