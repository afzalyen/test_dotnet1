using System;
using System.Collections.Generic;
using test_dotnet1_Models.Identity;

namespace test_dotnet1_Models.Models;
public class Question
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public DateTime CreatedAt { get; set; }

    // New properties for handling operations
    public bool IsDeleted { get; set; }
    public bool IsAnswered { get; set; }

    public ICollection<Answer> Answers { get; set; }
}