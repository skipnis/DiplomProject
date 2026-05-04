using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.FulfilledBadgeDefinitions.Update;

public record UpdateFulfilledBadgeDefinitionCommand(
    int Id,
    string Emoji,
    string Slug,
    string Label,
    string Description,
    bool IsActive) : ICommand;
