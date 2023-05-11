using Microsoft.AspNetCore.Mvc;

namespace ASP_201.Models.Forum
{
    public class ForumPostFormModel
    {
        [FromForm(Name = "post-content")]
        public String Content { get; set; } = null!;

        [FromForm(Name = "topic-id")]
        public String TopicId { get; set; } = null!;

        [FromForm(Name = "reply-id")]
        public String? ReplyId { get; set; }
    }
}
