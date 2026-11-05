using CourseHub.Api.Auth;
using CourseHub.Core.Data;
using CourseHub.Core.Domain;
using CourseHub.Core.Policies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseHub.Api.Controllers;

[ApiController]
[Authorize(Roles = "learner")]
public class EnrollmentsController : ControllerBase
{
    private readonly CourseHubDbContext _db;

    public EnrollmentsController(CourseHubDbContext db)
    {
        _db = db;
    }

    [HttpPost("api/courses/{courseId:int}/enrollments")]
    public async Task<IActionResult> Enroll(int courseId)
    {
        var learnerId = CallerId.From(User);
        if (string.IsNullOrEmpty(learnerId))
        {
            return Unauthorized();
        }

        var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
        if (course is null)
        {
            return NotFound(new { error = "Course not found" });
        }

        var existing = await _db.Enrollments.Where(e => e.CourseId == courseId).ToListAsync();
        var decision = EnrollmentPolicy.CanEnroll(course, learnerId, existing, DateTime.UtcNow);
        if (!decision.Allowed)
        {
            return Conflict(new { message = "Enrollment refused", code = decision.Reason });
        }

        var enrollment = new Enrollment
        {
            CourseId = courseId,
            LearnerId = learnerId,
            EnrolledAt = DateTime.UtcNow,
            Status = EnrollmentStatus.Active
        };

        _db.Enrollments.Add(enrollment);
        await _db.SaveChangesAsync();

        return Created($"/api/me/enrollments", new { enrollment.Id, enrollment.CourseId, enrollment.LearnerId });
    }

    [HttpDelete("api/courses/{courseId:int}/enrollments/me")]
    public async Task<IActionResult> Drop(int courseId)
    {
        var learnerId = CallerId.From(User);
        if (string.IsNullOrEmpty(learnerId))
        {
            return Unauthorized();
        }

        var enrollment = await _db.Enrollments.FirstOrDefaultAsync(e =>
            e.CourseId == courseId && e.LearnerId == learnerId && e.Status == EnrollmentStatus.Active);
        if (enrollment is null)
        {
            return NotFound(new { error = "Enrollment not found" });
        }

        enrollment.Status = EnrollmentStatus.Dropped;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("api/me/enrollments")]
    public async Task<IActionResult> Mine()
    {
        var learnerId = CallerId.From(User);
        if (string.IsNullOrEmpty(learnerId))
        {
            return Unauthorized();
        }

        var items = await _db.Enrollments
            .Where(e => e.LearnerId == learnerId)
            .Select(e => new { e.Id, e.CourseId, CourseTitle = e.Course!.Title, e.Status, e.EnrolledAt })
            .ToListAsync();
        return Ok(items);
    }
}
