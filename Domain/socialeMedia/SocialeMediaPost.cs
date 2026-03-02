using IntergratieProject.Domain.Ai;

namespace IntergratieProject.Domain.socialeMedia;

public class SocialMediaPost
{
        public int Id { get; set; }
        public ChannelType Channel { get; set; }
        public PostStatus Status { get; set; }
        public int AiInteractionId { get; set; }
        public AiIntergration AiInteraction { get; set; }
}