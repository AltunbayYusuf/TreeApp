using IntegratieProject.BL.Domain.Ai;

namespace IntegratieProject.BL.Domain.socialeMedia;

public class SocialMediaPost
{
        public int Id { get; set; }
        public ChannelType Channel { get; set; }
        public PostStatus Status { get; set; }
        public int AiInteractionId { get; set; }
        public AiIntegration AiIntegration { get; set; }
}