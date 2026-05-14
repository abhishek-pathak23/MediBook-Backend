namespace review_service.DTOs
{
    public class ReviewCreateDto
    {
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public int ProviderId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public bool IsAnonymous { get; set; }
    }

    public class ReviewResponseDto
    {
        public int ReviewId { get; set; }
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = "Anonymous Patient";
        public int ProviderId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime ReviewDate { get; set; }
        public bool IsVerified { get; set; }
        public bool IsAnonymous { get; set; }
    }

    public class AppointmentResponseDto
    {
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public int ProviderId { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
