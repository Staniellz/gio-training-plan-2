using CourseHub.Api.Auth;
using CourseHub.Core.Data;
using CourseHub.Core.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseHub.Api.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController : ControllerBase
{
    private readonly CourseHubDbContext _db;
    private readonly IAuthorizationService _authorization;

    public CoursesController(CourseHubDbContext db, IAuthorizationService authorization)
    {
        _db = db;
        _authorization = authorization;
    }

    [HttpGet]
    public ActionResult<IEnumerable<CourseSummary>> GetAll()
    {
        var courses = _db.Courses.OrderBy(c => c.Id).ToList();
        var result = new List<CourseSummary>();

        foreach (var course in courses)
        {
            var modules = _db.Modules.Where(m => m.CourseId == course.Id).ToList();
            var active = _db.Enrollments.Count(e => e.CourseId == course.Id && e.Status == EnrollmentStatus.Active);
            result.Add(new CourseSummary(
                course.Id,
                course.Title,
                course.InstructorId,
                course.IsPublished,
                modules.Count,
                modules.Sum(m => m.DurationMinutes),
                active));
        }

        return result;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CourseDetail>> GetById(int id)
    {
        var course = await _db.Courses.Include(c => c.Modules).FirstOrDefaultAsync(c => c.Id == id);
        if (course is null)
        {
            return NotFound(new { error = "Course not found" });
        }

        return new CourseDetail(
            course.Id,
            course.Title,
            course.Description,
            course.InstructorId,
            course.IsPublished,
            course.Capacity,
            course.EnrollmentClosesAt,
            course.Modules.OrderBy(m => m.Order)
                .Select(m => new ModuleItem(m.Id, m.Title, m.Order, m.DurationMinutes))
                .ToList());
    }

    [Authorize(Roles = "instructor")]
    [HttpPost]
    public async Task<ActionResult<CourseDetail>> Create(SaveCourseRequest request)
    {
        var userId = CallerId.From(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var course = new Course
        {
            Title = request.Title,
            Description = request.Description,
            IsPublished = request.IsPublished,
            Capacity = request.Capacity,
            EnrollmentClosesAt = request.EnrollmentClosesAt,
            InstructorId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Courses.Add(course);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = course.Id }, await GetById(course.Id));
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SaveCourseRequest request)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course is null)
        {
            return NotFound(new { error = "Course not found" });
        }

        var allowed = await _authorization.AuthorizeAsync(User, course, "CourseOwner");
        if (!allowed.Succeeded)
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        course.Title = request.Title;
        course.Description = request.Description;
        course.IsPublished = request.IsPublished;
        course.Capacity = request.Capacity;
        course.EnrollmentClosesAt = request.EnrollmentClosesAt;
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
