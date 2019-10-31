using System.ComponentModel.DataAnnotations;

namespace StudentExercisesAPI.Models
{
    public class Instructor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "You betta gimme a name")]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }

        [Required]
        [StringLength(25)]
        public string SlackHandle { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid cohort id")]
        public int CohortId { get; set; }
        public Cohort Cohort { get; set; }
    }
}
