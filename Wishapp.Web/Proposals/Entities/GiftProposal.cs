namespace Wishapp.Web.Proposals.Entities;

public sealed class GiftProposal
{
    public Guid Id { get; private set; }
    public Guid SenderId { get; private set; }
    public Guid RecipientId { get; private set; }
    public ProposalSourceType SourceType { get; private set; }
    public Guid? CatalogItemId { get; private set; }
    public Guid? WishlistItemId { get; private set; }
    public string? CustomTitle { get; private set; }
    public string? CustomDescription { get; private set; }
    public string? CustomImagePath { get; private set; }
    public string? HintMessage { get; private set; }
    public string? SenderAlias { get; private set; }
    public ProposalStatus Status { get; private set; }
    public string? RecipientComment { get; private set; }
    public bool IsViewedByRecipient { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ReactedAt { get; private set; }

    private GiftProposal() { }

    public static GiftProposal Create(
        Guid senderId,
        Guid recipientId,
        ProposalSourceType sourceType,
        Guid? catalogItemId,
        Guid? wishlistItemId,
        string? customTitle,
        string? customDescription,
        string? hintMessage,
        string? senderAlias)
    {
        return new GiftProposal
        {
            Id = Guid.CreateVersion7(),
            SenderId = senderId,
            RecipientId = recipientId,
            SourceType = sourceType,
            CatalogItemId = catalogItemId,
            WishlistItemId = wishlistItemId,
            CustomTitle = customTitle,
            CustomDescription = customDescription,
            HintMessage = hintMessage,
            SenderAlias = senderAlias,
            Status = ProposalStatus.Pending,
            IsViewedByRecipient = false,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void MarkViewed()
    {
        IsViewedByRecipient = true;
    }

    public void React(ProposalStatus status, string? comment)
    {
        Status = status;
        RecipientComment = comment;
        ReactedAt = DateTimeOffset.UtcNow;
    }

    public void SetCustomImage(string path)
    {
        CustomImagePath = path;
    }
}
