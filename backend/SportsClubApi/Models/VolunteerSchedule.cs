namespace SportsClubApi.Models;

public class VolunteerSchedule
{
    public int Id { get; set; }

    public int VolunteerId { get; set; }

    public DateTime StartDateTime { get; set; }

    public DateTime EndDateTime { get; set; }

    public string Activity { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string Status { get; set; } = "Scheduled";

    public Volunteer? Volunteer { get; set; }
}
